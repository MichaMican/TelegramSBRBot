using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Interfaces
{
    public interface ITelegramAPICommunicator
    {
        void SendMessage(string chatId, string message, string parse_mode = "html");
        void SendImage(string chatId, string imageURL, string caption = "", string parse_mode = "");
    }
}
