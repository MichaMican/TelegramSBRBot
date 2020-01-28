using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Interfaces
{
    public interface ICommandHandler
    {
        void HandleCommand(dynamic body);
    }
}
