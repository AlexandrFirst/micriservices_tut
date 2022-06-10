using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;

namespace PlatformService.SyncDataService.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
        }

        public async Task SendPlatformToCommand(PlatformReadDto plat)
        {
            var httpContent = new StringContent(JsonSerializer.Serialize(plat), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{configuration.GetSection("CommandService").Value}", httpContent);

            if(response.IsSuccessStatusCode)
            {
                System.Console.WriteLine("--> Sync POST to CommandService was OK!");
            }
            else
            {
                System.Console.WriteLine("--> Sync POST to CommandService was NOT OK!");
            }
        }
    }
}