using System.Security.Claims;
using CloudContactManager.Services.Interfaces;

namespace CloudContactManager.Services
{
    public class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public int GetTenantId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            if (int.TryParse(claim, out var tenantId))
            {
                return tenantId;
            }

            return _configuration.GetValue<int?>("Tenant:DefaultTenantId") ?? 1;
        }
    }
}
