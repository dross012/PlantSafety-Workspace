// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertToMetadataMapper.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Maps metadata operations into AlertMetadataEntry entities.
//      Supports Add, Update, and Remove operations in a deterministic,
//      domain-pure, additive-only manner.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Mappers;

public static class AlertToMetadataMapper
{
    // ------------------------------------------------------------------------
    // ADD
    // ------------------------------------------------------------------------
    public static AlertMetadataEntry Map(
        AlertId alertId,
        AlertMetadataKey key,
        AlertMetadataValue value)
    {
        return new AlertMetadataEntry(
            alertMetadataId: AlertMetadataId.New(),
            alertId: alertId,
            key: key,
            value: value);
    }

    // ------------------------------------------------------------------------
    // UPDATE
    // ------------------------------------------------------------------------
    public static AlertMetadataEntry MapUpdated(
        AlertId alertId,
        AlertMetadataKey key,
        AlertMetadataValue newValue)
    {
        return new AlertMetadataEntry(
            alertMetadataId: AlertMetadataId.New(),
            alertId: alertId,
            key: key,
            value: newValue);
    }

    // ------------------------------------------------------------------------
    // REMOVE
    // ------------------------------------------------------------------------
    public static AlertMetadataEntry MapRemoved(
        AlertId alertId,
        AlertMetadataKey key)
    {
        // Removal entries store a non-empty placeholder value to maintain
        // deterministic replay and snapshot consistency. AlertMetadataValue
        // deliberately forbids null/empty/whitespace (Rule 3.2), so a real
        // sentinel string is used instead of a true "empty" value.
        return new AlertMetadataEntry(
            alertMetadataId: AlertMetadataId.New(),
            alertId: alertId,
            key: key,
            value: new AlertMetadataValue("(removed)"));
    }
}

