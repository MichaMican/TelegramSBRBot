using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class CommandHandler : ICommandHandler
    {
        private readonly ITelegramAPICommunicator _telegramAPICommunicator;
        public CommandHandler(ITelegramAPICommunicator telegramAPICommunicator)
        {
            _telegramAPICommunicator = telegramAPICommunicator;
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
                for(int i = 0; i < entityArrayDynamic.Count; i++)
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

            foreach(var entity in entities)
            {
                if(entity.entityType == "bot_command")
                {
                    commandOffsetsList.Add(entity.offset);
                }
            }

            var commandOffsetsArray = commandOffsetsList.ToArray();

            string[] preprocessedCommands = new string[commandOffsetsArray.Length];

            int startPos = commandOffsetsArray[0]; //Message until first command can be ignored
            for (int i = 1; i < commandOffsetsArray.Length + 1; i++)
            {
                if(i < commandOffsetsArray.Length)
                {
                    preprocessedCommands[i - 1] = message.Substring(startPos, commandOffsetsArray[i] - startPos);
                    startPos = commandOffsetsArray[i];
                }
                else
                {
                    preprocessedCommands[i - 1] = message.Substring(startPos);
                }
            }

            foreach(string command in preprocessedCommands)
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

            //TODO handle the command
        }

        
    }
}
