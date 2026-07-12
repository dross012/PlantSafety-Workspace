// ============================================================================
// File: DeadLetterOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the integration dead-letter queue.
//              Bound from configuration section "Integrations:ErrorHandling:DeadLetter".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual dead-letter storage/replay logic
//   is implemented in the Infrastructure layer; this options class defines
//   the configuration surface the Infrastructure dead-letter handler binds
//   against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.ErrorHandling.DeadLetter;

/// <summary>Configuration options for the integration dead-letter queue.</summary>
public sealed class DeadLetterOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:ErrorHandling:DeadLetter";

    /// <summary>Whether dead-letter capture is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Maximum number of days a dead-lettered message is retained before purge.</summary>
    public int MaxRetentionDays { get; set; } = 30;
}
