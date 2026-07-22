// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the HazardCommunicationTraining aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="HazardCommunicationTraining"/> aggregate.</summary>
public static class HazardCommunicationTrainingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when training may be delivered from <paramref name="current"/>.</summary>
    public static bool CanDeliverTraining(HazardCommunicationTrainingStatus current) =>
        current is HazardCommunicationTrainingStatus.Scheduled or HazardCommunicationTrainingStatus.RetrainingRequired;

    /// <summary>Returns <see langword="true"/> when comprehension may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerifyComprehension(HazardCommunicationTrainingStatus current) => current == HazardCommunicationTrainingStatus.Delivered;

    /// <summary>Returns <see langword="true"/> when the next cycle may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleNextCycle(HazardCommunicationTrainingStatus current) => current == HazardCommunicationTrainingStatus.ComprehensionVerified;

    /// <summary>Returns <see langword="true"/> when the record may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(HazardCommunicationTrainingStatus current) => current == HazardCommunicationTrainingStatus.ComprehensionVerified;
}
