// ============================================================================
//  File:           IAlertFactory.cs
//  Created:        2026-05-26
//  Description:
//      Canonical factory for constructing Alerts and child entities.
//      Required by Canonical Aggregate Rule.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertFactory
{
    Alert Create(
        AlertType type,
        AlertSeverity severity,
        string title,
        string description,
        string sourceModule,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
