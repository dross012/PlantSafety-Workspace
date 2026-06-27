// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertRehydrationService.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical rehydration service for the Alert aggregate. Rebuilds an
//      Alert from a sequence of domain events in deterministic order.
//      Pure domain logic, no infrastructure concerns.
//
//      Complies with:
//          - Replay & Rebuild Rule
//          - Canonical Aggregate Rule (2.5)
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Behavior Rule (1.3)
//          - Additive-only evolution
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Interfaces;
using PlantSafety.Domain.Operational.Alerts.Mappers;

namespace PlantSafety.Domain.Operational.Alerts.Services;

public sealed class AlertRehydrationService : IAlertRehydrationService
{
    public Alert Rehydrate(IEnumerable<IAlertDomainEvent> events)
    {
        var ordered = events
            .OrderBy(e => e.OccurredAtUtc.Value)
            .ThenBy(e => e.EventId.Value) // deterministic tie-breaker
            .ToList();

        var alert = new Alert();

        foreach (var evt in ordered)
        {
            AlertEventToDomainMapper.Apply(alert, evt);
        }

        alert.ClearDomainEvents();

        return alert;
    }
}
