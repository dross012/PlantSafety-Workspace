// ============================================================================
//  File:           IAlertRehydrationService.cs
//  Created:        2026-05-26
//  Description:
//      Rebuilds an Alert aggregate from domain events.
//      Required by Replay & Rebuild Rule.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertRehydrationService
{
    Alert Rehydrate(IEnumerable<IAlertDomainEvent> events);
}

