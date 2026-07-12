// ============================================================================
// File: IntegrationJobOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for scheduled integration background jobs.
//              Bound from configuration section "Integrations:BackgroundServices:Jobs".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual job execution logic is implemented
//   in the Infrastructure layer; this options class defines the configuration
//   surface the Infrastructure job host binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.BackgroundServices.Jobs;

/// <summary>Configuration options for scheduled integration background jobs.</summary>
public sealed class IntegrationJobOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:BackgroundServices:Jobs";

    /// <summary>Whether integration jobs are enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Interval, in minutes, between successive job runs.</summary>
    public int IntervalMinutes { get; set; } = 15;

    /// <summary>Maximum number of retry attempts for a failed job run.</summary>
    public int MaxRetries { get; set; } = 3;
}
