// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCAStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for a ContinuousImprovement RCA (Root Cause Analysis) record.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at all
//                  -- only a placeholder marker class (RCAModuleMarker) and a read-only,
//                  date-range list query (GetRCAQuery/RCADapperRepository over
//                  usp_RCA_GetRcaRecords) existed. Values Initiated/InProgress/PendingApproval/
//                  Closed are taken verbatim from the vocabulary already documented on the
//                  pre-existing PlantSafety.Application.ContinuousImprovement.RCA.Models.RCAItemDto
//                  .Status ("e.g., Initiated/InProgress/PendingApproval/Closed") -- no new states
//                  were invented; Archived is added as the platform's standard terminal retention
//                  state, consistent with every other Phase 4 aggregate (e.g.
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Enums
//                  .RootCauseAnalysisStatus.Archived). This is a distinct type from
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Enums.RootCauseAnalysisStatus,
//                  which models the unrelated safety-incident root cause analysis bounded context
//                  backed by a different table ([SafetyEvents].[RootCauseAnalysis]); per the
//                  Maximalist Module Rule ("zero cross-module leakage"), ContinuousImprovement.RCA
//                  does not reference that enum. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContinuousImprovement.RCA.Enums;

/// <summary>Lifecycle states for a <see cref="Aggregates.RCA"/> (ContinuousImprovement root cause analysis) record.</summary>
public enum RCAStatus
{
    /// <summary>The RCA has been created/assigned; analysis has not started. Initial state on Create.</summary>
    Initiated = 0,

    /// <summary>The assigned owner is actively conducting the analysis.</summary>
    InProgress = 1,

    /// <summary>Analysis work is complete; findings recorded, awaiting sign-off/approval.</summary>
    PendingApproval = 2,

    /// <summary>The RCA has been approved and formally closed. Terminal state for active use.</summary>
    Closed = 3,

    /// <summary>The RCA has been archived for compliance retention.</summary>
    Archived = 4,
}
