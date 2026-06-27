// ============================================================================
//  File:           IAlertAutoClosePolicy.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertAutoClosePolicy
{
    bool ShouldAutoClose(Alert alert);
}
