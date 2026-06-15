namespace WebColegio.Services
{
    public class ApiTokenAccessor : IApiTokenAccessor
    {
        private const string SessionKey = "ApiJwtToken";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiTokenAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetToken(string token) =>
            _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, token);

        public string? GetToken() =>
            _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);

        public void ClearToken() =>
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }
}
