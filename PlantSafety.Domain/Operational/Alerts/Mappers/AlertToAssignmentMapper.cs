// ============================================================================
//  File:           AlertToAssignmentMapper.cs
//  Created:        2026-05-26
//  Description:
//      Maps assignment operations into AlertAssignment entities.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Mappers;

public static class AlertToAssignmentMapper
{
    public static AlertAssignment Map(
        AlertId alertId,
        string assignedTo,
        UtcTimestamp assignedAtUtc,
        string assignedBy)
    {
        return new AlertAssignment(
            alertAssignmentId: AlertAssignmentId.New(),
            alertId: alertId,
            assignedTo: assignedTo,
            assignedAtUtc: assignedAtUtc,
            assignedBy: assignedBy);
    }
}
