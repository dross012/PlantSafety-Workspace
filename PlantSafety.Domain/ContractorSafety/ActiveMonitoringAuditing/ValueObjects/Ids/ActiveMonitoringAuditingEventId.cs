// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for ActiveMonitoringAuditing domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ActiveMonitoringAuditing domain events.</summary>
public readonly record struct ActiveMonitoringAuditingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ActiveMonitoringAuditingEventId wrapping a non-empty GUID.</summary>
    public ActiveMonitoringAuditingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ActiveMonitoringAuditingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ActiveMonitoringAuditingEventId using Uuid7.</summary>
    public static ActiveMonitoringAuditingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}