// ============================================================================
// File: ExceptionRoutingOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for routing integration exceptions to
//              the appropriate handler/notification path. Bound from
//              configuration section "Integrations:ErrorHandling:ExceptionRouting".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual exception routing logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure exception router binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.ErrorHandling.ExceptionRouting;

/// <summary>Configuration options for routing integration exceptions to the appropriate handler.</summary>
public sealed class ExceptionRoutingOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:ErrorHandling:ExceptionRouting";

    /// <summary>Whether exception routing is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Whether stakeholders are notified when a routed exception occurs.</summary>
    public bool NotifyOnFailure { get; set; }
}
