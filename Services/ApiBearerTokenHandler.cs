using System.Net.Http.Headers;

namespace WebColegio.Services
{
    /// <summary>Adjunta Authorization: Bearer en cada llamada HTTP hacia la API.</summary>
    public class ApiBearerTokenHandler : DelegatingHandler
    {
        private readonly IApiTokenAccessor _tokenAccessor;

        public ApiBearerTokenHandler(IApiTokenAccessor tokenAccessor)
        {
            _tokenAccessor = tokenAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _tokenAccessor.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
