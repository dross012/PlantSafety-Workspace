// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           ActiveAlertsSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Concrete specification matching Alerts that have not yet been closed
//                  ("active" alerts, i.e. Triggered/Acknowledged/Dispatched/Escalated/Resolved).
//                  Additive-only evolution.
// ============================================================================
using AlertStatus = PlantSafety.Domain.Alerts.Alert.Enums.AlertStatus;

namespace PlantSafety.Domain.Alerts.Alert.Specifications;

/// <summary>Matches <see cref="Aggregates.Alert"/> aggregates that are not yet closed.</summary>
public sealed class ActiveAlertsSpecification : AlertSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.Alert, bool>> ToExpression()
        => alert => alert.Status != AlertStatus.Closed;
}
