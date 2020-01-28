using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TelegramFunFactBot.Classes.HelperClasses;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Controllers
{
    [Route("api/[controller]")]
    public class TelegramController : Controller
    {
        private readonly ICommandHandler _commandHandler;
        private readonly IDapperDB _dapperDB;

        public TelegramController(ICommandHandler commandHandler, IDapperDB dapperDB)
        {
            _commandHandler = commandHandler;
            _dapperDB = dapperDB;
        }

        [HttpPost("new-message")]
        public ActionResult NewMessage([FromBody]dynamic body)
        {
            _dapperDB.WriteRequestLog(body.ToString());
            _commandHandler.HandleCommand(body);
            return Ok();
        }
    }
}