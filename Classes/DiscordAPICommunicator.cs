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
        private readonly Settings _settings;
        public DiscordAPICommunicator(IOptions<Settings> settings)
        {
            _settings = settings.Value;
            _client.DefaultRequestHeaders.Add("ApiKey", _settings.dcMiddlewareApiKey);
        }

        public async Task AddRoleToUser(ulong userId, ulong roleId)
        {
            var url = $"https://discordtlgrmmiddleware.azurewebsites.net/api/users/{userId}/roles/{roleId}";
            await _client.PostAsync(url, null);
        }

        public async Task<List<DcUser>> GetUsersInChannel(ulong channelId)
        {
            var url = $"https://discordtlgrmmiddleware.azurewebsites.net/api/channels/{channelId}/users";

            var resRaw = await _client.GetAsync(url);
            var resString = await resRaw.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<List<DcUser>>(resString);

            return res;
        }

        public async Task RemoveRoleFromUser(ulong userId, ulong roleId)
        {
            var url = $"https://discordtlgrmmiddleware.azurewebsites.net/api/users/{userId}/roles/{roleId}";
            await _client.DeleteAsync(url);
        }
    }
}
