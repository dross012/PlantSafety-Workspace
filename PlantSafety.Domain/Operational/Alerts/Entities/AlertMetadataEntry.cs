// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertMetadataEntry.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Immutable entity representing a metadata key/value pair for an Alert.
//      Complies with:
//          - StrongId Enforcement Rule (5.5)
//          - ValueObject Rule (2.8)
//          - Canonical Aggregate Rule (2.5)
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertMetadataEntry
{
    public AlertMetadataId AlertMetadataId { get; }
    public AlertId AlertId { get; }
    public AlertMetadataKey Key { get; }
    public AlertMetadataValue Value { get; }

    public AlertMetadataEntry(
        AlertMetadataId alertMetadataId,
        AlertId alertId,
        AlertMetadataKey key,
        AlertMetadataValue value)
    {
        AlertMetadataId = alertMetadataId;
        AlertId = alertId;
        Key = key;
        Value = value;
    }
}
