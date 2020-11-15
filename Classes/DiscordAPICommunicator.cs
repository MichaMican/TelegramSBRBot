using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Classes.HelperClasses;
using TelegramFunFactBot.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TelegramFunFactBot.Models;

namespace TelegramFunFactBot.Classes
{
    public class DiscordAPICommunicator : IDiscordAPICommunicator
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly IDapperDB _dapperDB;
        private readonly ITelegramAPICommunicator _telegram;
        private readonly Settings _settings;
        public DiscordAPICommunicator(IOptions<Settings> settings, IDapperDB dapperDB, ITelegramAPICommunicator telegram)
        {
            _settings = settings.Value;
            _dapperDB = dapperDB;
            _telegram = telegram;
            _client.DefaultRequestHeaders.Add("ApiKey", _settings.dcMiddlewareApiKey);
        }

        public async Task AddRoleToUser(ulong userId, ulong roleId)
        {
            var url = $"https://discordtlgrmmiddleware.azurewebsites.net/api/users/{userId}/roles/{roleId}";
            try
            {
                await _client.PostAsync(url, null);
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("Discord API Com", "Error", e.Message);
                _telegram.SendErrorMessage($"Error while communicating with DC API {e.Message}");
            }
        }

        public async Task<List<DcUser>> GetUsersInChannel(ulong channelId)
        {
            try
            {
                var url = $"https://discordtlgrmmiddleware.azurewebsites.net/api/channels/{channelId}/users";

                var resRaw = await _client.GetAsync(url);
                var resString = await resRaw.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<List<DcUser>>(resString);

                return res;
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("Discord API Com", "Error", e.Message);
                _telegram.SendErrorMessage($"Error while communicating with DC API {e.Message}");
                return null;
            }

        }

        public async Task RemoveRoleFromUser(ulong userId, ulong roleId)
        {
            try
            {
                var url = $"https://discordtlgrmmiddleware.azurewebsites.net/api/users/{userId}/roles/{roleId}";
                await _client.DeleteAsync(url);
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("Discord API Com", "Error", e.Message);
                _telegram.SendErrorMessage($"Error while communicating with DC API {e.Message}");
            }
        }
    }
}
