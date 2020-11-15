using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Classes.Dapper.Tables;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class ReadyToPlayHandler : IReadyToPlayHandler
    {
        private readonly IDapperDB _dapperDB;
        private readonly ITelegramAPICommunicator _telegram;
        private readonly IDiscordAPICommunicator _dc;
        private readonly Settings _settings;
        public ReadyToPlayHandler(IOptions<Settings> settings, IDapperDB dapperDB, ITelegramAPICommunicator telegram, IDiscordAPICommunicator dc)
        {
            _settings = settings.Value;
            _dapperDB = dapperDB;
            _telegram = telegram;
            _dc = dc;
        }

        public async Task UpdateReadyToPlay()
        {
            var readyPlayers = await _dapperDB.GetReadyToPlayUsers();
            var dcUsersInAufAbruf = await _dc.GetUsersInChannel(_settings.dcAufAbrufId);

            foreach (var player in readyPlayers)
            {
                if (player.readyEndDate < DateTime.UtcNow)
                {
                    try
                    {
                        await _dc.RemoveRoleFromUser(Convert.ToUInt64(player.dcId), _settings.dcAufAbrufRoleId);
                    }
                    catch
                    {
                        //Fall through
                    }
                    await _dapperDB.DeleteReadyPlayer(player.tlgrmId);
                }
            };

            foreach (var dcUser in dcUsersInAufAbruf)
            {
                if (!readyPlayers.Where((e) => { return e.dcId == dcUser.id.ToString(); }).Any())
                {
                    string tlgrmId = null;
                    ConvertDict.DcID2TlgrmID.TryGetValue(dcUser.id.ToString(), out tlgrmId);

                    if (!String.IsNullOrEmpty(tlgrmId))
                    {
                        await _dapperDB.InsertReadyPlayer(new ReadyToPlayUsers()
                        {
                            tlgrmId = tlgrmId,
                            dcId = dcUser.id.ToString(),
                            readyStartDate = DateTime.UtcNow,
                            readyEndDate = DateTime.UtcNow.AddHours(1)
                        });

                        await _dc.AddRoleToUser(dcUser.id, _settings.dcAufAbrufRoleId);

                    }
                }
            }

            readyPlayers = await _dapperDB.GetReadyToPlayUsers();
            if(readyPlayers.Count == 4)
            {
                DateTime lastReadyPlayerNotification = (await _dapperDB.LoadFromDBStorage("lastReadyPlayerNotification")).dateTimeValue;
                if (lastReadyPlayerNotification == null || lastReadyPlayerNotification.AddMinutes(15) < DateTime.UtcNow)
                {
                    await _dapperDB.SaveToDBStorage(new DBStorage()
                    {
                        key = "lastReadyPlayerNotification",
                        dateTimeValue = DateTime.UtcNow
                    });

                    await _telegram.SendMessage(_settings.tlgrmSbrGroupId, await GetCurrentReadyStateString());
                }
            }

        }

        public async Task SetUserAufAbruf(string[] command, string userId)
        {
            if (ConvertDict.TlgrmID2DcID.ContainsKey(userId))
            {
                string dcId;
                int readyHours;
                DateTime readyEnds;

                var readyPlayers = await _dapperDB.GetReadyToPlayUsers();
                ConvertDict.TlgrmID2DcID.TryGetValue(userId, out dcId);

                if (command.Length >= 2 && Int32.TryParse(command[2], out readyHours))
                {
                    readyEnds = DateTime.UtcNow.AddHours(readyHours);
                }
                else
                {
                    readyEnds = DateTime.UtcNow.AddHours(1);
                }

                if (readyPlayers.Where(e => { return e.tlgrmId == userId; }).Any())
                {
                    //this has to block so user is deleted before he is added again
                    _dapperDB.DeleteReadyPlayer(userId).Wait();
                }
                else
                {
                    //only adds role if user wasn't already set as ready
                    await _dc.AddRoleToUser(UInt64.Parse(dcId), _settings.dcAufAbrufRoleId);
                }

                _dapperDB.InsertReadyPlayer(new ReadyToPlayUsers()
                {
                    tlgrmId = userId,
                    dcId = dcId,
                    readyStartDate = DateTime.UtcNow,
                    readyEndDate = readyEnds
                }).Wait();
                UpdateReadyToPlay().Wait();

                readyPlayers = await _dapperDB.GetReadyToPlayUsers();
                if(readyPlayers.Count >= 5)
                {
                    await _telegram.SendMessage(_settings.tlgrmSbrGroupId, await GetCurrentReadyStateString());
                }

            }
        }

        public async Task RemoveFromAufAbruf(string userId)
        {
            if (ConvertDict.TlgrmID2DcID.ContainsKey(userId))
            {
                string dcId;

                var readyPlayers = await _dapperDB.GetReadyToPlayUsers();
                ConvertDict.TlgrmID2DcID.TryGetValue(userId, out dcId);

                if (readyPlayers.Where(e => { return e.tlgrmId == userId; }).Any())
                {
                    await _dapperDB.DeleteReadyPlayer(userId);
                    await _dc.RemoveRoleFromUser(UInt64.Parse(dcId), _settings.dcAufAbrufRoleId);
                }

                await UpdateReadyToPlay();
            }
        }

        public async Task<string> GetCurrentReadyStateString()
        {
            var readyToPlayUsers = await _dapperDB.GetReadyToPlayUsers();
            string outputMessage = $"There are <b>{readyToPlayUsers.Count}/5 people ready</b> to play:\n";

            foreach(var player in readyToPlayUsers)
            {
                string playerName = "<error while name conversion>";
                ConvertDict.TlgrmID2Name.TryGetValue(player.tlgrmId, out playerName);
                outputMessage += $"<b>{playerName}</b> (Ready until {player.readyEndDate.ToString("hh:mm - dd.MM.yyyy")} (UTC))\n";
            }
            outputMessage += "\n\n <i>This info is updated every minute</i>";
            return outputMessage;
        }
    }
}
