// ============================================================================
//  File:           AlertSnapshotMapper.cs
//  Created:        2026-05-26
//  Description:
//      Maps an Alert aggregate into a snapshot object for rebuild or analytics.
// ============================================================================


// ============================================================================
//  File:           AlertSnapshotMapper.cs
//  Created:        2026-05-26
//  Description:
//      Maps an Alert aggregate into a snapshot object for rebuild or analytics.
// ============================================================================

using PlantSafety.Operational.Alerts.Domain.Alerts.Aggregates;

namespace PlantSafety.Domain.Operational.Alerts.Mappers;

public static class AlertSnapshotMapper
{
    public static AlertSnapshot Map(Alert alert)
    {
        return new AlertSnapshot(
            alertId: alert.AlertId,
            type: alert.AlertType,
            severity: alert.Severity,
            status: alert.Status,
            title: alert.Title,
            description: alert.Description,
            sourceModule: alert.SourceModule,
            createdAtUtc: alert.CreatedAtUtc,
            createdBy: alert.CreatedBy,
            acknowledgedAtUtc: alert.AcknowledgedAtUtc,
            inProgressAtUtc: alert.InProgressAtUtc,
            escalatedAtUtc: alert.EscalatedAtUtc,
            resolvedAtUtc: alert.ResolvedAtUtc,
            closedAtUtc: alert.ClosedAtUtc,
            archivedAtUtc: alert.ArchivedAtUtc,
            slaExpiration: alert.SlaExpiration,
            assignments: alert.Assignments.ToList(),
            metadata: alert.Metadata.ToList(),
            history: alert.History.ToList());
    }
}
