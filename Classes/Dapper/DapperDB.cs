﻿using Microsoft.Extensions.Options;
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
            try
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
            catch
            {
                //Fall through
            }
        }



        public async void WriteRequestLog(string jsonString)
        {
            try
            {
                var objToInsert = new RequestLog();
                if (jsonString.Length < 2000)
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
            catch
            {
                //Fall through
            }
        }

        public async void SubscribeToFunFacts(string chatId, DateTime nextUpdateOn)
        {
            try
            {
                var objToInsert = new FunFactSubscriber();
                objToInsert.chatId = chatId;
                objToInsert.nextUpdateOn = nextUpdateOn;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.InsertAsync(objToInsert);
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task<List<FunFactSubscriber>> GetFunFactSubscribers()
        {
            var listToReturn = new List<FunFactSubscriber>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    listToReturn = (await connection.GetAllAsync<FunFactSubscriber>()).ToList();
                }
            }
            catch
            {
                //Fall through
            }

            return listToReturn;
        }

        public async void UpdateFunFactNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.UpdateAsync(new FunFactSubscriber { chatId = chatId, nextUpdateOn = nextUpdateOn });
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async void UnsubscribeFromFunFacts(string chatId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.DeleteAsync(new FunFactSubscriber { chatId = chatId });
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task<string> GetCurrentVersion()
        {
            var response = new List<CurrentVersion>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    response = (await connection.GetAllAsync<CurrentVersion>()).ToList();
                }
            }
            catch
            {
                //Fall through
            }

            if (response.Count > 0)
            {
                var first = response.First();
                return first.version;
            }
            else
            {
                throw new Exception("No version was found in DB");
            }

        }

        public void UpdateVersion(string newVersion)
        {
            try
            {
                var versionUpdate = new CurrentVersion();
                versionUpdate.version = newVersion;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.DeleteAll<CurrentVersion>();


                    connection.Insert(versionUpdate);
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task<List<UpdateLogSubscriber>> GetAllUpdateSubscriber()
        {
            var returnList = new List<UpdateLogSubscriber>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    returnList = (await connection.GetAllAsync<UpdateLogSubscriber>()).ToList();
                }
            }
            catch
            {
                //Fall through
            }

            return returnList;
        }

        public async void SubscribeToUpdateLog(string chatId)
        {
            try
            {
                var objToInsert = new UpdateLogSubscriber();
                objToInsert.chatId = chatId;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.InsertAsync(objToInsert);
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async void UnsubscribeFromUpdateLog(string chatId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.DeleteAsync(new UpdateLogSubscriber { chatId = chatId });
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async void SubscribeToMemes(string chatId, DateTime nextUpdateOn)
        {
            try
            {
                var objToInsert = new MemeSubscriber();
                objToInsert.chatId = chatId;
                objToInsert.nextUpdateOn = nextUpdateOn;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.InsertAsync(objToInsert);
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async void UnsubscribeFromMemes(string chatId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.DeleteAsync(new MemeSubscriber { chatId = chatId });
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async void UpdateMemesNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.UpdateAsync(new MemeSubscriber { chatId = chatId, nextUpdateOn = nextUpdateOn });
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task<List<MemeSubscriber>> GetMemesSubscribers()
        {
            var listToReturn = new List<MemeSubscriber>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    listToReturn = (await connection.GetAllAsync<MemeSubscriber>()).ToList();
                }
            }
            catch
            {
                //Fall through
            }

            return listToReturn;
        }
    }
}
