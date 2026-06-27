// ============================================================================
// File: ApiOptions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Strongly-typed configuration options for PlantSafety.Api. Bound
//              from the "Api" configuration section in appsettings.json or Azure
//              Key Vault. Consumed by middleware, filters, and extension methods.
// ============================================================================
//
// Onboarding Notes:
// - Register via services.Configure<ApiOptions>(configuration.GetSection("Api"))
//   inside ServiceCollectionExtensions.AddPlantSafetyApi().
// - All sensitive values (client secrets, connection strings) should NOT appear
//   here — they belong in Key Vault and are referenced by the Infrastructure layer.
//   ApiOptions contains only non-secret API behaviour configuration.
// - DeprecatedVersions maps API version strings (e.g. "1.0") to their sunset
//   DateTimeOffset, consumed by ApiVersionDeprecationMiddleware.
// - RateLimit.RequestsPerWindow and WindowSeconds are consumed by
//   RateLimitingMiddleware.
// - MaintenanceMode bool is consumed by MaintenanceModeMiddleware.
//
// Rationale:
// - Centralising API configuration into a single typed options class prevents
//   string-keyed IConfiguration.GetValue<T>() calls scattered across middleware,
//   improving refactorability and IDE support.
//
// Future Extension Seams:
// - Add FeatureFlags: Dictionary<string, bool> for lightweight feature toggling
//   before a full Azure App Configuration integration is in place.
// - Add CorsOptions record for overriding the default CORS origin list at runtime.
//
// ============================================================================

namespace PlantSafety.Api.Configuration;

/// <summary>
/// Strongly-typed configuration options for PlantSafety.Api.
/// Bound from the <c>"Api"</c> section of application configuration.
/// </summary>
public sealed class ApiOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the API is in maintenance mode.
    /// When <c>true</c>, all non-health-check endpoints return HTTP 503.
    /// </summary>
    public bool MaintenanceMode { get; set; }

    /// <summary>
    /// Gets or sets the Azure Key Vault URI from which secrets are loaded at startup.
    /// Mirrors the <c>KeyVault:Uri</c> configuration key used in <c>Program.cs</c>.
    /// </summary>
    public string? KeyVaultUri { get; set; }

    /// <summary>
    /// Gets or sets the CORS allowed origins list for the PlantSafetyDefault policy.
    /// Leave empty to fall back to the default origins configured in
    /// <see cref="Extensions.ServiceCollectionExtensions"/>.
    /// </summary>
    public string[] AllowedCorsOrigins { get; set; } = [];

    /// <summary>Gets or sets the rate-limiting configuration.</summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Gets or sets the dictionary of deprecated API versions and their sunset dates.
    /// Key: API version string (e.g. <c>"1.0"</c>). Value: Sunset <see cref="DateTimeOffset"/>.
    /// Consumed by <see cref="Middleware.ApiVersionDeprecationMiddleware"/>.
    /// </summary>
    public Dictionary<string, DateTimeOffset> DeprecatedVersions { get; set; } = [];

    /// <summary>Gets or sets the Azure AD configuration for JWT bearer auth.</summary>
    public AzureAdOptions AzureAd { get; set; } = new();
}

/// <summary>Rate-limiting configuration nested inside <see cref="ApiOptions"/>.</summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Gets or sets the maximum number of requests allowed per tenant per window.
    /// Default: 300.
    /// </summary>
    public int RequestsPerWindow { get; set; } = 300;

    /// <summary>
    /// Gets or sets the sliding window duration in seconds.
    /// Default: 60.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;
}

/// <summary>Azure Active Directory options nested inside <see cref="ApiOptions"/>.</summary>
public sealed class AzureAdOptions
{
    /// <summary>Gets or sets the Azure AD authority URL (tenant-specific or common).</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>Gets or sets the API audience (Application ID URI or client ID).</summary>
    public string Audience { get; set; } = string.Empty;
}
