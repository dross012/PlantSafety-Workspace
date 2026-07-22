// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the PpePolicies aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PpePolicies"/> aggregate.</summary>
public static class PpePoliciesLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the policy may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(PpePoliciesStatus current) =>
        current is PpePoliciesStatus.Draft or PpePoliciesStatus.UnderRevision;

    /// <summary>Returns <see langword="true"/> when the policy may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(PpePoliciesStatus current) => current == PpePoliciesStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when an enforcement audit may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordEnforcementAudit(PpePoliciesStatus current) => current == PpePoliciesStatus.Published;

    /// <summary>Returns <see langword="true"/> when a revision may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleRevision(PpePoliciesStatus current) => current == PpePoliciesStatus.Published;
}
