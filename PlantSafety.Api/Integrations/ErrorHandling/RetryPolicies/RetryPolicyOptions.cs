// ============================================================================
// File: RetryPolicyOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for integration retry policies applied
//              to transient failures. Bound from configuration section
//              "Integrations:ErrorHandling:RetryPolicies".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual retry/backoff logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure retry policy binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.ErrorHandling.RetryPolicies;

/// <summary>Configuration options for integration retry policies applied to transient failures.</summary>
public sealed class RetryPolicyOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:ErrorHandling:RetryPolicies";

    /// <summary>Maximum number of retry attempts before a failure is escalated.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Base backoff delay, in seconds, applied between retry attempts.</summary>
    public int BackoffSeconds { get; set; } = 5;
}
