// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Alert aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using AlertStatus = PlantSafety.Domain.Alerts.Alert.Enums.AlertStatus;
using AlertCategory = PlantSafety.Domain.Alerts.Alert.Enums.AlertCategory;
using AlertSeverity = PlantSafety.Domain.Alerts.Alert.Enums.AlertSeverity;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Domain.Alerts.Alert.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.Alert"/> aggregate for fast rehydration.</summary>
public sealed record AlertSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AlertId AlertId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AlertStatus Status { get; init; }

    /// <summary>Catalog category code at snapshot time.</summary>
    public string CategoryCode { get; init; } = AlertCategory.Default.Code;

    /// <summary>Severity code at snapshot time.</summary>
    public string SeverityCode { get; init; } = AlertSeverity.Default.Code;

    /// <summary>UTC trigger timestamp.</summary>
    public UtcTimestamp TriggeredAtUtc { get; init; }
}
