using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class CommandHandler : ICommandHandler
    {
        private readonly ITelegramAPICommunicator _telegram;
        private readonly IDapperDB _dapperDB;
        private readonly IInit _init;
        private readonly IHttpHandler _httpHandler;
        private readonly Settings _settings;
        private readonly System.Threading.Timer _checkSubServicesThread;
        public CommandHandler(ITelegramAPICommunicator telegramAPICommunicator, IDapperDB dapperDB, IInit init, IOptions<Settings> settings, IHttpHandler httpHandler)
        {
            _httpHandler = httpHandler;
            _settings = settings.Value;
            _telegram = telegramAPICommunicator;
            _dapperDB = dapperDB;
            _init = init;
            _checkSubServicesThread = new System.Threading.Timer((e) =>
            {
                try
                {
                    _init.CheckForSubscribedServices();
                }
                catch
                {
                    /* Fall through */
                }
            }, null, 1000, 60000);
        }

        public class Entity
        {
            public int offset;
            public int lenght;
            public string entityType;

            public Entity(dynamic entityDyn)
            {
                offset = entityDyn.offset;
                lenght = entityDyn.length;
                entityType = entityDyn.type;
            }

            public static Entity[] convertDynamicToEntitiesArray(dynamic entityArrayDynamic)
            {
                var returnArray = new Entity[entityArrayDynamic.Count];
                for (int i = 0; i < entityArrayDynamic.Count; i++)
                {
                    returnArray[i] = new Entity(entityArrayDynamic[i]);
                }
                return returnArray;
            }
        }

        private List<string[]> DecodeCommand(string message, Entity[] entities)
        {
            var returnList = new List<string[]>();
            var commandOffsetsList = new List<int>();

            foreach (var entity in entities)
            {
                if (entity.entityType == "bot_command")
                {
                    commandOffsetsList.Add(entity.offset);
                }
            }

            var commandOffsetsArray = commandOffsetsList.ToArray();

            string[] preprocessedCommands = new string[commandOffsetsArray.Length];

            int startPos = commandOffsetsArray[0]; //Message until first command can be ignored
            for (int i = 1; i < commandOffsetsArray.Length + 1; i++)
            {
                if (i < commandOffsetsArray.Length)
                {
                    preprocessedCommands[i - 1] = message.Substring(startPos, commandOffsetsArray[i] - startPos);
                    startPos = commandOffsetsArray[i];
                }
                else
                {
                    preprocessedCommands[i - 1] = message.Substring(startPos);
                }
            }

            foreach (string command in preprocessedCommands)
            {
                returnList.Add(command.Split(" "));
            }


            return returnList;
        }

        public void HandleCommand(dynamic body)
        {
            List<string[]> commands = new List<string[]>();
            if (body.message.entities != null)
            {
                Entity[] entities = Entity.convertDynamicToEntitiesArray(body.message.entities);
                string messageText = body.message.text;
                commands = DecodeCommand(messageText, entities);
            }

            string chatId = body.message.chat.id;
            string chatType = body.message.chat.type;
            string username = body.message.from.username;
            string displayName = body.message.from.first_name + " " + body.message.from.last_name;


            foreach (string[] command in commands)
            {
                try
                {
                    switch (command[0])
                    {
                        case "/start":
                            SubscribeToUpdates(chatId);
                            _telegram.SendMessage(chatId, "Heyho :)");
                            break;
                        case "/help":
                        case "/help@sbrcs_bot":
                            SendHelp(chatId);
                            break;
                        case "/ping":
                        case "/ping@sbrcs_bot":
                            if (chatType == "private")
                            {
                                _telegram.SendMessage(chatId, "Pong");
                            }
                            break;
                        case "/unsubupdates":
                        case "/unsubupdates@sbrcs_bot":
                            UnsubscribeToUpdates(chatId);
                            _telegram.SendMessage(chatId, "Successfully unsubscribed from update log notification");
                            break;
                        case "/subfunfacts":
                        case "/subfunfacts@sbrcs_bot":
                            SubscribeToFunFacts(command, chatId);
                            _telegram.SendMessage(chatId, "Successfully subscribed to FunFacts by Joseph Paul");
                            break;
                        case "/unsubfunfacts":
                        case "/unsubfunfacts@sbrcs_bot":
                            UnsubscribeFromFunFacts(chatId);
                            _telegram.SendMessage(chatId, "Successfully unsubscribed from FunFacts");
                            break;
                        case "/submemes":
                        case "/submemes@sbrcs_bot":
                            SubscribeToMemes(command, chatId);
                            _telegram.SendMessage(chatId, "Successfully subscribed to RedditMemes");
                            break;
                        case "/unsubmemes":
                        case "/unsubmemes@sbrcs_bot":
                            UnsubscribeFromMemes(chatId);
                            _telegram.SendMessage(chatId, "Successfully unsubscribed from Memes");
                            break;
                        case "/idea":
                        case "/idea@sbrcs_bot":
                            if (chatType == "private")
                            {
                                newIdea(command, chatId, username, displayName);
                                //This is already done in newIdea function: _telegramAPICommunicator.SendMessage(chatId, "Your idea was submitted");
                            }
                            else
                            {
                                _telegram.SendMessage(chatId, "This command is only available in private conversations with the bot");
                            }
                            break;
                        default:
                            /* Fall through */
                            break;
                    }
                }
                catch
                {
                    _telegram.SendMessage(chatId, "There was an error while processing your command :(");
                }
            }
        }

        private void SendHelp(string chatId)
        {
            string allCommands = System.IO.File.ReadAllText(@"./VERSIONLOG/allCommands.txt");

            _telegram.SendMessage(chatId, allCommands);
        }

        private void SubscribeToUpdates(string chatId)
        {
            _dapperDB.SubscribeToUpdateLog(chatId);
        }

        private void UnsubscribeToUpdates(string chatId)
        {
            _dapperDB.UnsubscribeFromFunFacts(chatId);
        }

        private void UnsubscribeFromFunFacts(string chatId)
        {
            _dapperDB.UnsubscribeFromFunFacts(chatId);
        }

        private void SubscribeToFunFacts(string[] command, string chatId)
        {
            DateTime timeToUpdate = DateTime.Now;

            try
            {
                var time = command[1].Split(":");
                if (time.Length == 2)
                {
                    int hours = Int32.Parse(time[0]);
                    int minutes = Int32.Parse(time[1]);

                    if ((hours >= 0 && hours <= 24) && (minutes >= 0 && minutes <= 59))
                    {
                        timeToUpdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, minutes, 0);
                        if (timeToUpdate < DateTime.Now)
                        {
                            timeToUpdate = timeToUpdate.AddDays(1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                /*Fall through*/
            }

            _dapperDB.SubscribeToFunFacts(chatId, timeToUpdate);
        }
        
        private void UnsubscribeFromMemes(string chatId)
        {
            _dapperDB.UnsubscribeFromMemes(chatId);
        }

        private void SubscribeToMemes(string[] command, string chatId)
        {
            DateTime timeToUpdate = DateTime.Now;

            try
            {
                var time = command[1].Split(":");
                if (time.Length == 2)
                {
                    int hours = Int32.Parse(time[0]);
                    int minutes = Int32.Parse(time[1]);

                    if ((hours >= 0 && hours <= 24) && (minutes >= 0 && minutes <= 59))
                    {
                        timeToUpdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, minutes, 0);
                        if (timeToUpdate < DateTime.Now)
                        {
                            timeToUpdate = timeToUpdate.AddDays(1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                /*Fall through*/
            }

            _dapperDB.SubscribeToMemes(chatId, timeToUpdate);
        }

        private void newIdea(string[] command, string chatId, string userName, string displayName)
        {
            try
            {
                string title = command[1];
                string description = "";

                for (int i = 2; i < command.Length; i++)
                {
                    description += command[i] + " ";
                }

                string ideaMessage = "Idea: <b>" + title + "</b>\nDescription: " + description + "\n------------------------------------------------------------\nThis idea was submitted by @" + userName + " (" + displayName + ")";

                _telegram.SendMessage(_settings.ideaChatId, ideaMessage);
                _telegram.SendMessage(chatId, "Your idea was submitted");
            }
            catch
            {
                _telegram.SendMessage(chatId, "There was an error while processing your request. Make sure you use the correct syntax: /idea [Title] [Description]");
            }

        }

    }
}
