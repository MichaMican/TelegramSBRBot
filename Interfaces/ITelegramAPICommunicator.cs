using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Models;

namespace TelegramFunFactBot.Interfaces
{
    public interface ITelegramAPICommunicator
    {
        Task<Message> SendMessage(string chatId, string message, string parse_mode = "html");
        void SendImage(string chatId, string imageURL, string caption = "", string parse_mode = "");
        void UpdateMessage(string chatId, int messageId, string text, string parse_mode = "html");
        void SendErrorMessage(string errorMessage);
        void LeaveChat(string chatId);
    }
}
