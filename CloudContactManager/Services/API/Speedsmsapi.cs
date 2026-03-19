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
            var sender = _configuration["SpeedSMS:Sender"]; // cần cho sms_type = 2

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("SpeedSMS:AccessToken is missing");
                return false;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Phone number or message is empty");
                return false;
            }

            // Normalize phone
            var normalizedPhone = phoneNumber.Trim();
            if (normalizedPhone.StartsWith("+84"))
            {
                normalizedPhone = normalizedPhone.Substring(1);
            }
            else if (normalizedPhone.StartsWith("0") && normalizedPhone.Length == 10)
            {
                normalizedPhone = "84" + normalizedPhone.Substring(1);
            }

            if (!normalizedPhone.StartsWith("84"))
            {
                _logger.LogWarning("Invalid VN phone format: {Phone}", phoneNumber);
                return false;
            }

            // ⚠️ fallback strategy
            var useOtp = true;

            var payload = useOtp
                ? new
                {
                    to = new[] { normalizedPhone },
                    content = message,
                    sms_type = 4
                }
                : new
                {
                    to = new[] { normalizedPhone },
                    content = message,
                    sms_type = 2,
                    sender = sender
                };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var raw = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accessToken}:"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);

            _logger.LogInformation("SpeedSMS request: {Payload}", json);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("SpeedSMS response {StatusCode}: {Body}", (int)response.StatusCode, body);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var status = root.GetProperty("status").GetString();

                if (status == "success")
                {
                    return true;
                }

                var messageErr = root.TryGetProperty("message", out var msg) ? msg.GetString() : null;

                _logger.LogWarning("SpeedSMS error: {Message}", messageErr);

                // 🔥 fallback nếu OTP fail
                if (useOtp)
                {
                    _logger.LogWarning("Retry with sms_type = 2");

                    var fallbackPayload = new
                    {
                        to = new[] { normalizedPhone },
                        content = message,
                        sms_type = 2,
                        sender = sender
                    };

                    var fallbackJson = JsonSerializer.Serialize(fallbackPayload);

                    using var retryRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send")
                    {
                        Content = new StringContent(fallbackJson, Encoding.UTF8, "application/json")
                    };

                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);

                    var retryResponse = await _httpClient.SendAsync(retryRequest, cancellationToken);
                    var retryBody = await retryResponse.Content.ReadAsStringAsync(cancellationToken);

                    _logger.LogInformation("Retry response: {Body}", retryBody);

                    using var retryDoc = JsonDocument.Parse(retryBody);
                    return retryDoc.RootElement.GetProperty("status").GetString() == "success";
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse response");
                return false;
            }
        }
    }
}