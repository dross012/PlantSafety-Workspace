// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic CRUD lifecycle with the real 3-phase risk
//                  management process documented in "Full Risk Management Process (Detailed &
//                  Structured)": PHASE 1 Risk Assessment (identify/score/prioritize/document),
//                  PHASE 2 Risk Mitigation (apply controls/implement/validate), PHASE 3 Risk
//                  Reporting & Review (report/monitor/review).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the Risk aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.RiskManagement.Enums;

/// <summary>Lifecycle states for the Risk aggregate root.</summary>
public enum RiskStatus
{
    /// <summary>Hazard has been identified (Phase 1, Step 1) but not yet scored.</summary>
    Identified = 0,

    /// <summary>Severity x Likelihood scoring has been recorded (Phase 1, Step 2).</summary>
    Scored = 1,

    /// <summary>A P1-P4 priority has been assigned and the finding documented (Phase 1, Steps 3-4).</summary>
    Prioritized = 2,

    /// <summary>One or more control measures (Hierarchy of Controls) are being applied (Phase 2, Steps 5-6).</summary>
    ControlsInProgress = 3,

    /// <summary>Controls are applied; residual risk has been validated as acceptable (Phase 2, Step 7).</summary>
    Mitigated = 4,

    /// <summary>Risk is undergoing its periodic monitoring/review cycle (Phase 3, Steps 9-10).</summary>
    UnderReview = 5,

    /// <summary>Risk has been formally closed; no further action required.</summary>
    Closed = 6,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 7,
}
