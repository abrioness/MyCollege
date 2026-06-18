using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace WebColegio.Services
{
    /// <summary>Adjunta Authorization: Bearer en cada llamada HTTP hacia la API.</summary>
    public class ApiBearerTokenHandler : DelegatingHandler
    {
        private readonly IApiTokenAccessor _tokenAccessor;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiBearerTokenHandler> _logger;

        public ApiBearerTokenHandler(
            IApiTokenAccessor tokenAccessor,
            IJwtTokenService jwtTokenService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiBearerTokenHandler> logger)
        {
            _tokenAccessor = tokenAccessor;
            _jwtTokenService = jwtTokenService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.Session != null && !ctx.Session.IsAvailable)
                await ctx.Session.LoadAsync(cancellationToken);

            string? token = null;

            // Token fresco desde la cookie de login (evita JWT vencido guardado en sesión)
            if (ctx?.User?.Identity?.IsAuthenticated == true)
            {
                token = _jwtTokenService.TryCreateTokenFromUser(ctx.User);
                if (!string.IsNullOrWhiteSpace(token))
                    _tokenAccessor.SetToken(token);
            }

            if (string.IsNullOrWhiteSpace(token))
                token = _tokenAccessor.GetToken();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning(
                    "Llamada a la API sin token JWT: {Method} {Url}. Inicie sesión de nuevo o verifique Jwt:SecretKey.",
                    request.Method,
                    request.RequestUri);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning(
                    "API respondió 401 Unauthorized en {Url}. Verifique que Jwt:SecretKey, Issuer y Audience sean iguales en Web y API.",
                    request.RequestUri);
            }

            return response;
        }
    }
}
