using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CloudContactManager.Services.API
{
    public class Speedsmsapi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public Speedsmsapi(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            var accessToken = _configuration["SpeedSMS:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return false;
            }

            var sender = _configuration["SpeedSMS:Sender"] ?? "CloudContact";
            var payload = new
            {
                to = new[] { phoneNumber },
                content = message,
                sms_type = 2,
                sender
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}
