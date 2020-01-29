using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using TelegramFunFactBot.Interfaces;
using TelegramFunFactBot.Classes.Dapper.Tables;

namespace TelegramFunFactBot.Classes.Dapper
{
    public class DapperDB : IDapperDB
    {
        private readonly Settings _settings;
        private SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

        public DapperDB(IOptions<Settings> settings)
        {
            _settings = settings.Value;
            builder.DataSource = _settings.dbServer;
            builder.UserID = _settings.dbUser;
            builder.Password = _settings.dbPwd;
            builder.InitialCatalog = _settings.dbInitCat;
        }

        public async void WriteEventLog(string source, string type, string message, string logGroup = null)
        {
            var objToInsert = new EventLog();
            objToInsert.source = source;
            objToInsert.type = type;
            objToInsert.logGroup = logGroup;
            objToInsert.message = message;
            objToInsert.timestamp = DateTime.UtcNow;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.InsertAsync(objToInsert);
            }
        }

        public async void WriteRequestLog(string jsonString)
        {
            var objToInsert = new RequestLog();
            if(jsonString.Length < 2000)
            {
                objToInsert.requestJson = jsonString;
            } 
            else
            {
                WriteEventLog("Dapper", "Error", "A Request Json was to long to be saved in DB");
            }
            objToInsert.timestamp = DateTime.UtcNow;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.InsertAsync(objToInsert);
            }
        }

        public async void SubscribeToFunFacts(string chatId, DateTime nextUpdateOn)
        {
            var objToInsert = new FunFactSubscriber();
            objToInsert.chatId = chatId;
            objToInsert.nextUpdateOn = nextUpdateOn;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.InsertAsync(objToInsert);
            }
        }

        public async Task<List<FunFactSubscriber>> GetFunFactSubscribers()
        {
            var listToReturn = new List<FunFactSubscriber>();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                listToReturn = (await connection.GetAllAsync<FunFactSubscriber>()).ToList();
            }

            return listToReturn;
        }

        public async void UpdateFunFactNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.UpdateAsync(new FunFactSubscriber { chatId = chatId, nextUpdateOn = nextUpdateOn });
            }
        }

        public async void UnsubscribeFromFunFacts(string chatId)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.DeleteAsync(new FunFactSubscriber { chatId = chatId });
            }
        }
    }
}
