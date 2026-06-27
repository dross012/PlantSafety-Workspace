// ============================================================================
//  File:           IAlertArchivalPolicy.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Interfaces;

public interface IAlertArchivalPolicy
{
    bool ShouldArchive(Alert alert);
}
