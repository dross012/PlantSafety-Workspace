// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real training-cycle lifecycle states for the HazardCommunicationTraining aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Enums;

/// <summary>Lifecycle states for the <see cref="HazardCommunicationTraining"/> aggregate root.</summary>
public enum HazardCommunicationTrainingStatus
{
    /// <summary>Training session scheduled but not yet delivered.</summary>
    Scheduled = 0,

    /// <summary>Training has been delivered to employees.</summary>
    Delivered = 1,

    /// <summary>Employee comprehension verified (quiz/assessment passed).</summary>
    ComprehensionVerified = 2,

    /// <summary>Comprehension check failed; retraining required.</summary>
    RetrainingRequired = 3,

    /// <summary>Training cycle closed out (superseded by next cycle or program retired).</summary>
    Closed = 4,
}
