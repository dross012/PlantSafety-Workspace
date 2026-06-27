// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertToHistoryMapper.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Maps lifecycle transitions into AlertHistoryEntry entities.
//      Pure domain logic. No infrastructure dependencies.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Mappers;

public static class AlertToHistoryMapper
{
    public static AlertHistoryEntry Map(
        AlertId alertId,
        AlertStatus previous,
        AlertStatus next,
        UtcTimestamp changedAtUtc,
        string changedBy,
        string? notes)
    {
        return new AlertHistoryEntry(
            alertHistoryId: AlertHistoryId.New(),
            alertId: alertId,
            previousStatus: previous,
            newStatus: next,
            changedAtUtc: changedAtUtc,
            changedBy: changedBy,
            notes: notes);
    }
}
