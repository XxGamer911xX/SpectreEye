using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AntiCheat.Core
{
    public static class ApiClient
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<ApiResponse> RegisterSessionAsync(string serial, string hwid)
        {
            var payload = new
            {
                serial = serial,
                hardware_id = hwid
            };

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "http://localhost/anticheat/register_session.php",
                content
            );

            string body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(body);
        }
        public static async Task<bool> SendHeartbeatAsync(string sessionToken, string serial, string HWID)
        {
            var payload = new
            {
                serial = serial,
                hardware_id = HWID,
                session_token = sessionToken
            };

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "http://localhost/anticheat/heartbeat.php",
                content
            );

            return response.IsSuccessStatusCode;
        }
    }


    public class ApiResponse
    {
        public string status { get; set; }
        public string session_token { get; set; }
        public string serial { get; set; }
        public string hardware_id { get; set; }
        public bool modified { get; set; }
    }

}
