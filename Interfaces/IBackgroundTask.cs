﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Interfaces
{
    public interface IBackgroundTask
    {
        void CheckForSubscribedServices();
    }
}
