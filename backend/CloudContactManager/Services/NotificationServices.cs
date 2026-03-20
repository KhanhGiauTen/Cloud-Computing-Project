using CloudContactManager.Services.API;

namespace CloudContactManager.Services
{
    public class NotificationServices
    {
        private readonly Speedsmsapi _speedSmsApi;

        public NotificationServices(Speedsmsapi speedSmsApi)
        {
            _speedSmsApi = speedSmsApi;
        }

        public Task<bool> SendSmsAsync(string? phoneNumber, string? message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                return Task.FromResult(false);
            }

            try
            {
                // Gửi theo kiểu CSKH (TYPE_CSKH) và không dùng brandname để tránh lỗi sender
                var json = _speedSmsApi.sendSMS(
                    new[] { phoneNumber },
                    message,
                    Speedsmsapi.TYPE_CSKH,
                    string.Empty);

                var ok = false;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("status", out var statusProp))
                        {
                            var status = statusProp.GetString();
                            ok = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    catch
                    {
                        ok = false;
                    }
                }

                return Task.FromResult(ok);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
