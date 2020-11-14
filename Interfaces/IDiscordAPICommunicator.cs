using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Models;

namespace TelegramFunFactBot.Interfaces
{
    public interface IDiscordAPICommunicator
    {
        Task<List<DcUser>> GetUsersInChannel(ulong userId);
        Task AddRoleToUser(ulong userId, ulong roleId);
        Task RemoveRoleFromUser(ulong userId, ulong roleId);
    }
}
