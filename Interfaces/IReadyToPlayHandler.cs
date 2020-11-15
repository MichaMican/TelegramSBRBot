using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Interfaces
{
    public interface IReadyToPlayHandler
    {
        Task UpdateReadyToPlay();
        Task SetUserAufAbruf(string[] command, string userId);
        Task RemoveFromAufAbruf(string userId);
        Task<string> GetCurrentReadyStateString();
    }
}
