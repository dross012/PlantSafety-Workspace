// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ActiveMonitoringAuditing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ActiveMonitoringAuditing aggregate root.</summary>
public readonly record struct ActiveMonitoringAuditingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ActiveMonitoringAuditingId wrapping a non-empty GUID.</summary>
    public ActiveMonitoringAuditingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ActiveMonitoringAuditingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ActiveMonitoringAuditingId using Uuid7.</summary>
    public static ActiveMonitoringAuditingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ActiveMonitoringAuditingId.</summary>
    public static ActiveMonitoringAuditingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ActiveMonitoringAuditingId.</summary>
    public static ActiveMonitoringAuditingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}