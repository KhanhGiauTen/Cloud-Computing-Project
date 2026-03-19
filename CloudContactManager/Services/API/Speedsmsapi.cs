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

            // Decide SMS type and sender based on configuration.
            // If a Device is configured, use type 5 (Android device) with deviceId as sender.
            // Otherwise, use type 2 (random number) and optional configured Sender/brandname.
            var deviceId = _configuration["SpeedSMS:Device"];
            int smsType;
            string sender;

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                smsType = 5; // send via Android device
                sender = deviceId;
            }
            else
            {
                smsType = 2; // random number
                sender = _configuration["SpeedSMS:Sender"] ?? "CloudContact";
            }

            var payload = new
            {
                to = new[] { phoneNumber },
                content = message,
                sms_type = smsType,
                sender
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            // SpeedSMS uses HTTP Basic auth with the token as username and empty password.
            // Authorization: Basic base64("<token>:")
            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accessToken}:"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Non-2xx from SpeedSMS => consider as failure
                return false;
            }

            // SpeedSMS returns JSON: { "status": "success" | "error", "code": "..", ... }
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("status", out var statusProp))
                {
                    var status = statusProp.GetString();
                    return string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                // If response is not valid JSON or unexpected, treat as failure
                return false;
            }

            return false;
        }
    }
}
