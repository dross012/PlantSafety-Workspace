// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PermitWorkflow aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Policies;

/// <summary>Determines whether a <see cref="PermitWorkflow"/> aggregate is eligible for archival.</summary>
public static class PermitWorkflowArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PermitWorkflow may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate)
        => aggregate.Status == PermitWorkflowStatus.Closed;
}