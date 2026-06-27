// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: MSAL-backed token provider that acquires Azure AD access
//   tokens for the PlantSafety API scope. Implements silent acquisition with
//   interactive fallback. Token cache integrated with platform-specific secure
//   storage via MSAL.Extensions.Maui.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.0
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace PlantSafety.UI.Infrastructure.Auth;

/// <summary>
/// Acquires and caches Azure AD bearer tokens for the PlantSafety API.
/// Uses MSAL silent acquisition first, falling back to interactive device-code
/// flow when the cache is empty or the token is expired.
/// </summary>
/// <remarks>
/// Configuration keys (appsettings.json / platform secret store):
///   AzureAd:Authority  — AAD authority URL including tenant ID
///   AzureAd:ClientId   — registered application (client) ID
///   AzureAd:Scopes     — comma-separated API permission scopes
/// </remarks>
public sealed class MsalTokenProvider : IMsalTokenProvider
{
    private readonly IPublicClientApplication _msalClient;
    private readonly IReadOnlyList<string> _scopes;
    private readonly ILogger<MsalTokenProvider> _logger;

    /// <summary>
    /// Initializes the MSAL public client with platform-specific cache integration.
    /// </summary>
    /// <param name="configuration">Application configuration providing AzureAd settings.</param>
    /// <param name="logger">Structured logger; PII is never emitted.</param>
    public MsalTokenProvider(IConfiguration configuration, ILogger<MsalTokenProvider> logger)
    {
        _logger = logger;

        var authority = configuration["AzureAd:Authority"]
            ?? throw new InvalidOperationException("AzureAd:Authority configuration is missing.");

        var clientId = configuration["AzureAd:ClientId"]
            ?? throw new InvalidOperationException("AzureAd:ClientId configuration is missing.");

        var rawScopes = configuration["AzureAd:Scopes"] ?? string.Empty;
        _scopes = rawScopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .ToList()
                           .AsReadOnly();

        _msalClient = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(authority)
            .WithRedirectUri("msal://auth")
            .Build();
    }

    /// <summary>
    /// Acquires an access token silently from the MSAL cache or interactively if
    /// the cache is empty or the stored token has expired.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>A valid bearer access token string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when token acquisition fails after all retry strategies are exhausted.
    /// </exception>
    public async Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            var firstAccount = accounts.FirstOrDefault();

            var result = await _msalClient
                .AcquireTokenSilent(_scopes, firstAccount)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("MSAL silent token acquisition succeeded. ExpiresOn={ExpiresOn}", result.ExpiresOn);
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            _logger.LogInformation("MSAL silent acquisition requires interaction; initiating interactive flow.");
            return await AcquireTokenInteractiveAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                "MSAL token acquisition failed. ExceptionType={ExceptionType} Message={Message}",
                ex.GetType().Name,
                ex.Message);

            throw new InvalidOperationException("Failed to acquire authentication token.", ex);
        }
    }

    /// <summary>
    /// Removes all cached accounts and tokens, triggering a full re-authentication
    /// on the next <see cref="AcquireTokenAsync"/> call.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
        foreach (var account in accounts)
        {
            await _msalClient.RemoveAsync(account).ConfigureAwait(false);
        }

        _logger.LogInformation("MSAL sign-out completed. All cached accounts removed.");
    }

    private async Task<string> AcquireTokenInteractiveAsync(CancellationToken cancellationToken)
    {
        var result = await _msalClient
            .AcquireTokenInteractive(_scopes)
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("MSAL interactive token acquisition succeeded.");
        return result.AccessToken;
    }
}
