using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TelegramFunFactBot.Classes.Dapper.Tables;
using TelegramFunFactBot.Classes.HelperClasses;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Controllers
{
    [Route("api")]
    public class TelegramController : Controller
    {
        private readonly ICommandHandler _commandHandler;
        private readonly IDapperDB _dapperDB;
        private readonly IDiscordAPICommunicator _dc;
        private readonly ITelegramAPICommunicator _telegramAPI;
        private readonly IBackgroundTask _backgroundTask;
        private readonly IReadyToPlayHandler _readyToPlayHandler;
        private readonly Settings _settings;
        private readonly List<long> _blockedUsers = new List<long>()
        {
            -1001353479498
        };

        public TelegramController(IOptions<Settings> settings, IReadyToPlayHandler readyToPlayHandler, IBackgroundTask backgroundTask, IDiscordAPICommunicator dc, ICommandHandler commandHandler, IDapperDB dapperDB, ITelegramAPICommunicator telegramAPI)
        {
            _settings = settings.Value;
            _backgroundTask = backgroundTask;
            _commandHandler = commandHandler;
            _dapperDB = dapperDB;
            _telegramAPI = telegramAPI;
            _readyToPlayHandler = readyToPlayHandler;
            _dc = dc;
        }

        [HttpPost("telegram/new-message")]
        public ActionResult NewMessage([FromBody]dynamic body)
        {
            try
            {
                //only look at messages from chats (not from eg. Channels)
                if (body.message != null)
                {
                    if (!_blockedUsers.Contains(Convert.ToInt64(body.message.chat.id)))
                    {
                        _commandHandler.HandleCommand(body);
                        _dapperDB.WriteRequestLog(body.ToString());
                    }
                } else if (body.channel_post != null)
                {
                    _telegramAPI.LeaveChat(body.channel_post.chat.id);
                    _dapperDB.UnsubscribeFromUpdateLog(body.channel_post.chat.id);
                }
                
            }
            catch (Exception e)
            {
                try
                {
                    _dapperDB.WriteEventLog("TelegramControler", "Error", "There was an dangorous error in the Controller!" + e.Message);
                }
                catch
                {
                    /*Fall through*/
                }
                return Ok();
            }

            return Ok();
        }
        
        [HttpPost("discord/sync")]
        public async Task<ActionResult> UpdateDC()
        {
            await _readyToPlayHandler.UpdateReadyToPlay();
            return Ok();
        }
    }
}