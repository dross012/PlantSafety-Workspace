// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Delegating HTTP handler that attaches the Azure AD bearer
//   token to every outbound API request. Must be registered after
//   CorrelationHandler in the DI pipeline so correlation headers are already
//   present when the token is injected.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.0
// ============================================================

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.Auth;

namespace PlantSafety.UI.Infrastructure.Auth;

/// <summary>
/// Delegating handler that acquires a bearer token from <see cref="IMsalTokenProvider"/>
/// and stamps the Authorization header on every outbound request to the PlantSafety API.
/// </summary>
public sealed class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IMsalTokenProvider _tokenProvider;
    private readonly ILogger<AuthorizationMessageHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthorizationMessageHandler"/>.
    /// </summary>
    /// <param name="tokenProvider">The MSAL token provider.</param>
    /// <param name="logger">Structured logger; tokens are never logged.</param>
    public AuthorizationMessageHandler(IMsalTokenProvider tokenProvider, ILogger<AuthorizationMessageHandler> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.AcquireTokenAsync(cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _logger.LogDebug("Authorization header attached to outbound request. Method={Method} Uri={Uri}",
            request.Method,
            request.RequestUri?.AbsolutePath);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
