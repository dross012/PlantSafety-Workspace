// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SystemHealth
//  Layer:          Application
//  File:           SystemHealthSectionDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Generic resilience wrapper for every section of the System Health
//                  rollup report. Each section of GetSystemHealthReportQueryHandler is
//                  fetched independently; if the underlying sub-query/repository call
//                  fails, the section is marked unavailable with a sanitized reason
//                  instead of failing the entire report. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.SystemHealth.Models;

/// <summary>
/// Wraps a single section of the System Health rollup report with an availability flag,
/// so one broken data source degrades gracefully instead of blanking the whole dashboard.
/// </summary>
/// <typeparam name="T">The section's payload type.</typeparam>
/// <param name="IsAvailable">Whether this section's data was retrieved successfully.</param>
/// <param name="Data">The section payload. Default when <paramref name="IsAvailable"/> is false.</param>
/// <param name="UnavailableReason">Sanitized, user-safe explanation when unavailable. Null when available.</param>
public sealed record SystemHealthSectionDto<T>(bool IsAvailable, T? Data, string? UnavailableReason)
{
    /// <summary>Creates an available section wrapping the given data.</summary>
    public static SystemHealthSectionDto<T> Available(T data) => new(true, data, null);

    /// <summary>Creates an unavailable section with a sanitized reason.</summary>
    public static SystemHealthSectionDto<T> Unavailable(string reason) => new(false, default, reason);
}
