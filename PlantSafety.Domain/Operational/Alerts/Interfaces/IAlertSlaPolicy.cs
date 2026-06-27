// ============================================================================
//  File:           IAlertSlaPolicy.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertSlaPolicy
{
    UtcTimestamp CalculateExpiration(Alert alert);
}
