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

        private async Task Subscribe<T>(string chatId, DateTime nextUpdateOn) where T : Subscribeable, new()
        {
            try
            {
                T objToInsert = new T();
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

        public async Task Unsubscribe<T>(string chatId) where T : Subscribeable, new()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.DeleteAsync(new T { chatId = chatId });
                }
            }
            catch
            {
                //Fall through
            }
        }


        public async Task UpdateNextUpdateOn<T>(string chatId, DateTime nextUpdateOn) where T : Subscribeable, new()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.UpdateAsync(new T { chatId = chatId, nextUpdateOn = nextUpdateOn });
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task<List<T>> GetSubscribers<T>() where T : Subscribeable, new()
        {
            var listToReturn = new List<T>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    listToReturn = (await connection.GetAllAsync<T>()).ToList();
                }
            }
            catch
            {
                //Fall through
            }

            return listToReturn;
        }


        public async void SubscribeToFunFacts(string chatId, DateTime nextUpdateOn)
        {
            await Subscribe<FunFactSubscriber>(chatId, nextUpdateOn);
        }
        public async void UnsubscribeFromFunFacts(string chatId)
        {
            await Unsubscribe<FunFactSubscriber>(chatId);
        }
        public async void UpdateFunFactNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            await UpdateNextUpdateOn<FunFactSubscriber>(chatId, nextUpdateOn);
        }
        public async Task<List<FunFactSubscriber>> GetFunFactSubscribers()
        {
            return await GetSubscribers<FunFactSubscriber>();
        }


        public async void SubscribeToMemes(string chatId, DateTime nextUpdateOn)
        {
            await Subscribe<MemeSubscriber>(chatId, nextUpdateOn);
        }
        public async void UnsubscribeFromMemes(string chatId)
        {
            await Unsubscribe<MemeSubscriber>(chatId);
        }
        public async void UpdateMemesNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            await UpdateNextUpdateOn<MemeSubscriber>(chatId, nextUpdateOn);
        }
        public async Task<List<MemeSubscriber>> GetMemesSubscribers()
        {
            return await GetSubscribers<MemeSubscriber>();
        }


        public async void SubscribeToDeutscheMemes(string chatId, DateTime nextUpdateOn)
        {
            await Subscribe<DeutscheMemeSubscriber>(chatId, nextUpdateOn);
        }
        public async void UnsubscribeFromDeutscheMemes(string chatId)
        {
            await Unsubscribe<DeutscheMemeSubscriber>(chatId);
        }
        public async void UpdateDeutscheMemesNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            await UpdateNextUpdateOn<DeutscheMemeSubscriber>(chatId, nextUpdateOn);
        }
        public async Task<List<DeutscheMemeSubscriber>> GetDeutscheMemesSubscribers()
        {
            return await GetSubscribers<DeutscheMemeSubscriber>();
        }

        public async void SetCountdown(string chatId, string title, DateTime countdownEnd, int messageId)
        {
            var countdownToSet = new Countdown();
            countdownToSet.countdownEnd = countdownEnd;
            countdownToSet.messageId = messageId;
            countdownToSet.title = title;
            countdownToSet.chatId = chatId;
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.InsertAsync(countdownToSet);
                }
            }
            catch (Exception e)
            {
                WriteEventLog("Dapper", "Error", "Could not insert into Countdown table Error: " + e.Message);
            }
        }

        public async Task<List<Countdown>> GetAllCountdowns()
        {
            var returnList = new List<Countdown>();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    returnList = (await connection.GetAllAsync<Countdown>()).ToList();
                }
                catch (Exception e)
                {
                    WriteEventLog("Dapper", "Error", "Could not read countdown table! Error: " + e.Message);
                }
            }

            return returnList;
        }

        public async void StopCountdown(int messageId)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    await connection.DeleteAsync(new Countdown() { messageId=messageId });
                }
                catch (Exception e)
                {
                    WriteEventLog("Dapper", "Error", "Could not remove element from countdown table! Error: " + e.Message);
                }
            }
        }

        public async void SubscribeToCsgoUpdates(string chatId)
        {
            await Subscribe<CSGOUpdatesSubscriber>(chatId, DateTime.Now);
        }

        public async void UnsubscribeFromCsgoUpdates(string chatId)
        {
            await Unsubscribe<CSGOUpdatesSubscriber>(chatId);
        }

        public async Task<List<CSGOUpdatesSubscriber>> GetAllCsgoUpdateSubscriber()
        {
            return await GetSubscribers<CSGOUpdatesSubscriber>();
        }

        public async Task SaveToDBStorage(DBStorage keyValuePair)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                bool keyIsInDB = false;
                try
                {
                    var res = (await connection.QueryAsync<DBStorage>("SELECT * FROM DBStorage WHERE [key]=@key", new { key = keyValuePair.key })).ToList();
                    keyIsInDB |= res.Count > 0;
                }
                catch (Exception e)
                {
                    //Fall through
                }
                if (keyIsInDB)
                {
                    try
                    {
                        await connection.UpdateAsync(keyValuePair);
                    }
                    catch (Exception e)
                    {
                        WriteEventLog("Dapper", "Error", "Could not Save to DBStorage (update)! Error: " + e.Message);
                        throw e;
                    }
                }
                else
                {
                    try
                    {
                        await connection.InsertAsync(keyValuePair);
                    }
                    catch (Exception e)
                    {
                        WriteEventLog("Dapper", "Error", "Could not Save to DBStorage (Inser)! Error: " + e.Message);
                        throw e;
                    }
                }
                
            }
        }

        public async Task<DBStorage> LoadFromDBStorage(string key)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    return (await connection.QueryAsync<DBStorage>("SELECT * FROM DBStorage WHERE [key] = @key", new { key = key })).ToList().First();
                }
                catch (Exception e)
                {
                    WriteEventLog("Dapper", "Error", "Could Load value from DBStorage! Error: " + e.Message);
                }
                return null;
            }
        }

        public async Task<List<DuckSubscriber>> GetAllDuckSubscriber()
        {
            return await GetSubscribers<DuckSubscriber>();
        }

        public async void SubscribeToDucks(string chatId, DateTime timeToUpdate)
        {
            await Subscribe<DuckSubscriber>(chatId, timeToUpdate);
        }

        public async void UnsubscribeToDucks(string chatId)
        {
            await Unsubscribe<DuckSubscriber>(chatId);
        }

        public async void UpdateDucksNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            await UpdateNextUpdateOn<DuckSubscriber>(chatId, nextUpdateOn);
        }

        public async Task<List<AlpacaSubscriber>> GetAllAlpacaSubscriber()
        {
            return await GetSubscribers<AlpacaSubscriber>();
        }

        public async void UnsubscribeFromAlpacasAsync(string chatId)
        {
            await Unsubscribe<AlpacaSubscriber>(chatId);
        }

        public async void SubscribeToAlpacasAsync(string chatId, DateTime timeToUpdate)
        {
            await Subscribe<AlpacaSubscriber>(chatId, timeToUpdate);
        }

        public async void UpdateAlpacasNextUpdateOn(string chatId, DateTime nextUpdateOn)
        {
            await UpdateNextUpdateOn<AlpacaSubscriber>(chatId, nextUpdateOn);
        }

        public async Task<List<ReadyToPlayUsers>> GetReadyToPlayUsers()
        {
            var listToReturn = new List<ReadyToPlayUsers>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    listToReturn = (await connection.GetAllAsync<ReadyToPlayUsers>()).ToList();
                }
            }
            catch
            {
                //Fall through
            }

            return listToReturn;
        }

        public async Task ClearReadyPlayersWhichReachedEndDate()
        {
            try
            {
                string sql = "DELETE FROM ReadyToPlayUsers WHERE readyEndDate < GETDATE()";
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.ExecuteAsync(sql);
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task ResetReadyPlayers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.DeleteAllAsync<ReadyToPlayUsers>();
                }
            }
            catch
            {
                //Fall through
            }
        }

        public async Task InsertReadyPlayer(ReadyToPlayUsers user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.InsertAsync(user);
                }
            }
            catch (Exception e)
            {
                //Fall through
            }
        }

        public async Task DeleteReadyPlayer(string tlgrmId)
        {
            try
            {
                string sql = "DELETE FROM ReadyToPlayUsers WHERE tlgrmId = @tlgrmId";
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.ExecuteAsync(sql, new { tlgrmId = tlgrmId });
                }
            }
            catch
            {
                //Fall through
            }
        }
    }
}
