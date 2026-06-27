// ============================================================================
//  File:           IAlertEscalationPolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining escalation rules.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertEscalationPolicy
{
    bool CanEscalate(Alert alert);
}
