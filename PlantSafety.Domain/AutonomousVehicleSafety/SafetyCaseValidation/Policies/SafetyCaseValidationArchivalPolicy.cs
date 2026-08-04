// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for SafetyCaseValidation aggregates.
//                  Superseded/expired safety-case document versions remain retained for the audit
//                  trail per the module's data-governance policy; only rejected cases archive
//                  immediately. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Policies;

/// <summary>Determines whether a <see cref="Aggregates.SafetyCaseValidation"/> aggregate is eligible for archival.</summary>
public static class SafetyCaseValidationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the safety-case record may be archived (rejected or expired).</summary>
    public static bool IsEligibleForArchival(Aggregates.SafetyCaseValidation aggregate)
        => aggregate.Status is SafetyCaseValidationStatus.Rejected or SafetyCaseValidationStatus.Expired;
}
