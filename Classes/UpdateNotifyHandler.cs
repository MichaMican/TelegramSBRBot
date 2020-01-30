using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class UpdateNotifyHandler : IUpdateNotifyHandler
    {
        private readonly IDapperDB _dapperDB;
        private readonly ITelegramAPICommunicator _telegram;
        public UpdateNotifyHandler(IDapperDB dapperDB, ITelegramAPICommunicator telegram)
        {
            _telegram = telegram;
            _dapperDB = dapperDB;
        }

        public async void checkForUpdates()
        {
            try
            {
                string versionFromFile = System.IO.File.ReadAllText(@"./VERSIONLOG/currentVersion.txt").Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                string versionInDB = (await _dapperDB.GetCurrentVersion()).Replace(" ", string.Empty);

                //Check if there is a new version
                if (versionFromFile != versionInDB)
                {
                    _dapperDB.UpdateVersion(versionFromFile);
                    string updateLog = System.IO.File.ReadAllText(@"./VERSIONLOG/currentUpdateLog.txt");

                    string updateMessage = "<b>v" + versionFromFile + "</b> \n --------------------------------------- \n" + updateLog;

                    var subscribers = await _dapperDB.GetAllUpdateSubscriber();

                    foreach (var sub in subscribers)
                    {
                        _telegram.SendMessage(sub.chatId, updateMessage);
                    }
                }
            } 
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("UpdateNotifyHandler", "ERROR", "There was an Error while checkingForUpdateNotifing" + e.Message, "checkForUpdates");
            }
        }
    }
}
