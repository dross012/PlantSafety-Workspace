// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the HazardCommunicationRightToKnow
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="HazardCommunicationRightToKnow"/> aggregate.</summary>
public static class HazardCommunicationRightToKnowLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the procedure may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(HazardCommunicationRightToKnowStatus current) =>
        current is HazardCommunicationRightToKnowStatus.Draft or HazardCommunicationRightToKnowStatus.UnderRevision;

    /// <summary>Returns <see langword="true"/> when the procedure may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(HazardCommunicationRightToKnowStatus current) => current == HazardCommunicationRightToKnowStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when a revision may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleRevision(HazardCommunicationRightToKnowStatus current) => current == HazardCommunicationRightToKnowStatus.Published;
}
