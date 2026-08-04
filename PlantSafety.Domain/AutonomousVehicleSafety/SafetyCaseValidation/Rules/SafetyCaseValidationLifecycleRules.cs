// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the SafetyCaseValidation aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.SafetyCaseValidation"/> aggregate.</summary>
public static class SafetyCaseValidationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the case may be validated from <paramref name="current"/>.</summary>
    public static bool CanValidate(SafetyCaseValidationStatus current) => current == SafetyCaseValidationStatus.PendingReview;

    /// <summary>Returns <see langword="true"/> when the case may be rejected from <paramref name="current"/>.</summary>
    public static bool CanReject(SafetyCaseValidationStatus current) => current == SafetyCaseValidationStatus.PendingReview;

    /// <summary>Returns <see langword="true"/> when the case may expire from <paramref name="current"/>.</summary>
    public static bool CanExpire(SafetyCaseValidationStatus current) => current == SafetyCaseValidationStatus.Validated;
}
