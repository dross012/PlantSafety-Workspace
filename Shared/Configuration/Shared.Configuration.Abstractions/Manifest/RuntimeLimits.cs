// ============================================================================
//  Project:        Shared.Configuration
//  Module:         Abstractions.Manifest
//  File:           RuntimeLimits.cs
//  Created:        2026-08-09
//  Description:    Phase 2.5 (Configuration & Environment Hardening):
//                  the runtime limits a host enforces. Real values, not
//                  placeholders -- PlantSafety.UI.Api applies these to
//                  Kestrel (MaxRequestBodySizeBytes) and ASP.NET Core's
//                  request-timeout middleware (RequestTimeout) at startup.
// ============================================================================
namespace Shared.Configuration.Abstractions.Manifest;

/// <summary>The runtime limits a host enforces.</summary>
public sealed record RuntimeLimits(long MaxRequestBodySizeBytes, TimeSpan RequestTimeout, int MaxConcurrentRequests)
{
    /// <summary>Sane defaults if none are configured: 10 MB body, 30s timeout, 100 concurrent requests.</summary>
    public static RuntimeLimits Default { get; } = new(MaxRequestBodySizeBytes: 10 * 1024 * 1024, RequestTimeout: TimeSpan.FromSeconds(30), MaxConcurrentRequests: 100);
}
