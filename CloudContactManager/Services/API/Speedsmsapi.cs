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

            // Chuẩn hóa số điện thoại về dạng 84xxxxxxxxx (yêu cầu của SpeedSMS, không có '+').
            var normalizedPhone = phoneNumber.Trim();
            if (normalizedPhone.StartsWith("+84"))
            {
                // +8493xxxxxxx -> 8493xxxxxxx
                normalizedPhone = normalizedPhone.Substring(1);
            }
            else if (normalizedPhone.StartsWith("0") && normalizedPhone.Length == 10)
            {
                // 09xxxxxxxx -> 84xxxxxxxxx
                normalizedPhone = "84" + normalizedPhone.Substring(1);
            }

            if (!normalizedPhone.StartsWith("84"))
            {
                _logger.LogWarning("Invalid VN phone format for SpeedSMS: {Phone}", phoneNumber);
                return false;
            }

            // Dùng sms_type = 4 cho OTP/verify theo hướng dẫn SpeedSMS.
            const int smsType = 4;
            var sender = _configuration["SpeedSMS:Sender"]; // có thể cấu hình "Verify" hoặc "Notify"
            if (string.IsNullOrWhiteSpace(sender))
            {
                // SpeedSMS dùng brandname mặc định Verify/Notify cho type=4. Dùng "Verify" nếu không cấu hình.
                sender = "Verify";
            }

            var payload = new
            {
                to = new[] { normalizedPhone },
                content = message,
                sms_type = smsType,
                sender
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // SpeedSMS uses HTTP Basic auth with the token as username and empty password.
            // Authorization: Basic base64("<token>:")
            var raw = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accessToken}:"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);

            _logger.LogInformation("SpeedSMS request to {Phone}: {Payload}", normalizedPhone, json);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("SpeedSMS response {StatusCode} for {Phone}: {Body}",
                (int)response.StatusCode, normalizedPhone, body);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SpeedSMS returned non-success HTTP status: {StatusCode}", (int)response.StatusCode);
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
                    if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    var code = root.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
                    var msg = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;

                    _logger.LogWarning("SpeedSMS returned error status for {Phone}. status={Status}, code={Code}, message={Message}",
                        normalizedPhone, status, code, msg);
                    return false;
                }

                _logger.LogWarning("SpeedSMS response has no 'status' field. Body: {Body}", body);
                return false;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse SpeedSMS JSON response: {Body}", body);
                return false;
            }
        }
    }
}
