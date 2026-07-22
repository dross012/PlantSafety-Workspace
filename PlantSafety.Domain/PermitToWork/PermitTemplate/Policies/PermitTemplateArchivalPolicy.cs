// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PermitTemplate aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Policies;

/// <summary>Determines whether a <see cref="PermitTemplate"/> aggregate is eligible for archival.</summary>
public static class PermitTemplateArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PermitTemplate may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate aggregate)
        => aggregate.Status == PermitTemplateStatus.Closed;
}