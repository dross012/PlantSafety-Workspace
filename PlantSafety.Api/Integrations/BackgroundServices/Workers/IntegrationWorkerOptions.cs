// ============================================================================
// File: IntegrationWorkerOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for integration background worker
//              processes. Bound from configuration section
//              "Integrations:BackgroundServices:Workers".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual worker execution logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure worker host binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.BackgroundServices.Workers;

/// <summary>Configuration options for integration background worker processes.</summary>
public sealed class IntegrationWorkerOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:BackgroundServices:Workers";

    /// <summary>Whether integration background workers are enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Maximum number of worker tasks allowed to run concurrently.</summary>
    public int ConcurrencyLimit { get; set; } = 4;
}
