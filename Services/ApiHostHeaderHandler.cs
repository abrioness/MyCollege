namespace WebColegio.Services
{
    /// <summary>
    /// Cuando la Web llama a 127.0.0.1/localhost, IIS necesita el Host del sitio publicado
    /// (p. ej. colegioparroquialsanfranciscojavier.com) para enrutar a la aplicación virtual de la API.
    /// </summary>
    public class ApiHostHeaderHandler : DelegatingHandler
    {
        private readonly string? _host;

        public ApiHostHeaderHandler(IConfiguration configuration)
        {
            _host = configuration["ApiSettings:Host"]
                ?? Environment.GetEnvironmentVariable("ApiSettings__Host");
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestHost = request.RequestUri?.Host;
            var needsHostOverride = requestHost is "127.0.0.1" or "localhost"
                || string.Equals(requestHost, "::1", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(_host) && needsHostOverride)
            {
                request.Headers.Host = _host.Trim();
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
