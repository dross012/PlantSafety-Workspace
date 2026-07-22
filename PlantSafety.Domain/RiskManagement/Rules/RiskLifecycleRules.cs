// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskLifecycleRules.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- realigned with the real 3-phase risk management process
//                  (Identified -> Scored -> Prioritized -> ControlsInProgress -> Mitigated ->
//                  UnderReview -> Closed -> Archived).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Risk aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Enums;

namespace PlantSafety.Domain.RiskManagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the Risk aggregate.</summary>
public static class RiskLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the risk may be scored from <paramref name="current"/>.</summary>
    public static bool CanScore(RiskStatus current) => current is RiskStatus.Identified or RiskStatus.Scored;

    /// <summary>Returns <see langword="true"/> when the risk may be prioritized from <paramref name="current"/>.</summary>
    public static bool CanPrioritize(RiskStatus current) => current == RiskStatus.Scored;

    /// <summary>Returns <see langword="true"/> when a control measure may be applied from <paramref name="current"/>.</summary>
    public static bool CanApplyControlMeasure(RiskStatus current)
        => current is RiskStatus.Prioritized or RiskStatus.ControlsInProgress;

    /// <summary>Returns <see langword="true"/> when effectiveness may be validated from <paramref name="current"/>.</summary>
    public static bool CanValidateEffectiveness(RiskStatus current) => current == RiskStatus.ControlsInProgress;

    /// <summary>Returns <see langword="true"/> when the risk may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(RiskStatus current) => current is RiskStatus.Mitigated or RiskStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(RiskStatus current) => current == RiskStatus.Closed;
}
