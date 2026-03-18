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

        public async Task<bool> SendSmsAsync(string? phoneNumber, string? message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            return await _speedSmsApi.SendSmsAsync(phoneNumber, message, cancellationToken);
        }
    }
}
