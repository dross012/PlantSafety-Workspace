// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           SchedulableTaskId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for a schedulable unit of work fed into the
//                  predictive scheduling rule engine. Wraps the identifier of the source
//                  aggregate (e.g. PMScheduling, WorkOrderPriority, HazardIsolationSequencing).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a schedulable task fed into the rule engine.</summary>
public readonly record struct SchedulableTaskId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SchedulableTaskId wrapping a non-empty GUID.</summary>
    public SchedulableTaskId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SchedulableTaskId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SchedulableTaskId using Uuid7.</summary>
    public static SchedulableTaskId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID (typically the source aggregate identifier) as a SchedulableTaskId.</summary>
    public static SchedulableTaskId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
