// ============================================================================
//  File:           IAlertEventRegistry.cs
//  Created:        2026-05-26
//  Description:
//      Maps event type names and versions to CLR types.
//      Required for replay, rebuild, and outbox consumption.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertEventRegistry
{
    Type? Resolve(string eventType, int version);
}
