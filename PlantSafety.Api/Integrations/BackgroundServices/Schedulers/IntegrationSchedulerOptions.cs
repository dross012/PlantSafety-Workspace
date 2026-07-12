// ============================================================================
// File: IntegrationSchedulerOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the integration background task
//              scheduler. Bound from configuration section
//              "Integrations:BackgroundServices:Schedulers".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual scheduling logic is implemented
//   in the Infrastructure layer; this options class defines the configuration
//   surface the Infrastructure scheduler binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.BackgroundServices.Schedulers;

/// <summary>Configuration options for the integration background task scheduler.</summary>
public sealed class IntegrationSchedulerOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:BackgroundServices:Schedulers";

    /// <summary>Whether the integration scheduler is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Cron expression governing when scheduled integration tasks fire.</summary>
    public string? CronExpression { get; set; }
}
