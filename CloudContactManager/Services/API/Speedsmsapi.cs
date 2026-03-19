using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CloudContactManager.Services.API
{
    public class Speedsmsapi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Speedsmsapi> _logger;

        public Speedsmsapi(HttpClient httpClient, IConfiguration configuration, ILogger<Speedsmsapi> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            var accessToken = _configuration["SpeedSMS:AccessToken"];
            var sender = _configuration["SpeedSMS:Sender"];

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Missing AccessToken");
                return false;
            }

            // Normalize phone
            var normalizedPhone = phoneNumber.Trim();
            if (normalizedPhone.StartsWith("+84"))
                normalizedPhone = normalizedPhone.Substring(1);
            else if (normalizedPhone.StartsWith("0") && normalizedPhone.Length == 10)
                normalizedPhone = "84" + normalizedPhone.Substring(1);

            object payload;

            bool useOtp = true;

            if (useOtp)
            {
                payload = new
                {
                    to = new[] { normalizedPhone },
                    content = message,
                    sms_type = 4
                };
            }
            else
            {
                payload = new
                {
                    to = new[] { normalizedPhone },
                    content = message,
                    sms_type = 2,
                    sender = sender
                };
            }

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send");

            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            request.Content = content;

            var raw = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accessToken}:"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);

            _logger.LogInformation("Payload: {Payload}", json);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Response: {Body}", body);

            return body.Contains("success");
        }
    }
}