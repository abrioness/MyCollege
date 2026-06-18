using Microsoft.Extensions.Options;
using WebColegio.Configuration;

namespace WebColegio.Services
{
    public class ApiTokenAccessor : IApiTokenAccessor
    {
        public const string CookieName = "ColegioApiJwt";
        private const string SessionKey = "ApiJwtToken";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSettings _jwtSettings;

        public ApiTokenAccessor(IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtSettings = jwtSettings.Value;
        }

        public void SetToken(string token)
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
                return;

            ctx.Session.SetString(SessionKey, token);

            ctx.Response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = ctx.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes > 0
                    ? _jwtSettings.ExpirationMinutes
                    : 480)
            });
        }

        public string? GetToken()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
                return null;

            var fromSession = ctx.Session.GetString(SessionKey);
            if (!string.IsNullOrWhiteSpace(fromSession))
                return fromSession;

            if (ctx.Request.Cookies.TryGetValue(CookieName, out var fromCookie) &&
                !string.IsNullOrWhiteSpace(fromCookie))
                return fromCookie;

            return null;
        }

        public void ClearToken()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
                return;

            ctx.Session.Remove(SessionKey);
            ctx.Response.Cookies.Delete(CookieName);
        }
    }
}
