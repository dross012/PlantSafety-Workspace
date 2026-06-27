// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Identity
//  Layer:          Infrastructure
//  File:           TokenValidationService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Azure AD bearer token validation using Microsoft.IdentityModel.Tokens.
//                  Additive-only evolution.
// ============================================================================
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Validates Azure AD bearer tokens against the Entra ID OIDC discovery endpoint.
/// Validates: issuer, audience, signature, and expiry.
/// </summary>
public sealed class TokenValidationService(IOptions<AzureAdOptions> opts, ILogger<TokenValidationService> logger)
    : ITokenValidationService
{
    private readonly AzureAdOptions _opts = opts.Value;
    private readonly JwtSecurityTokenHandler _handler = new();

    /// <inheritdoc/>
    public async Task<ClaimsPrincipal> ValidateAsync(string bearerToken, CancellationToken cancellationToken = default)
    {
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{_opts.Authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());

        var oidcConfig = await configManager.GetConfigurationAsync(cancellationToken);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = $"https://sts.windows.net/{_opts.TenantId}/",
            ValidateAudience         = true,
            ValidAudience            = _opts.Audience,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys        = oidcConfig.SigningKeys,
            ClockSkew                = TimeSpan.FromMinutes(5),
        };

        var principal = _handler.ValidateToken(bearerToken, validationParams, out _);
        logger.LogDebug("Token validated for subject {Subject}", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        return principal;
    }
}
