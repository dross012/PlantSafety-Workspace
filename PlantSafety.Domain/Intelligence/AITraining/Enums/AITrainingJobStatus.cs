// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for an Intelligence.AITraining AITrainingJob record. Phase 4
//                  full build-out: this bounded context previously had no aggregate at all -- only
//                  a placeholder marker class (AITrainingModuleMarker) and a read-only date-range
//                  list query (GetAITrainingQuery/AITrainingDapperRepository over
//                  usp_AITraining_GetTrainingJobs) existed. Values Queued/Running/Completed/Failed
//                  are taken verbatim from the vocabulary already documented on the pre-existing
//                  PlantSafety.Application.Intelligence.AITraining.Models.AITrainingItemDto.Status
//                  ("e.g., Queued/Running/Completed/Failed") -- no new states were invented;
//                  Archived is added as the platform's standard terminal retention state, consistent
//                  with every other Phase 4 aggregate (e.g.
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Enums.RCAStatus.Archived).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AITraining.Enums;

/// <summary>Lifecycle states for an <see cref="Aggregates.AITrainingJob"/> record.</summary>
public enum AITrainingJobStatus
{
    /// <summary>The training job has been created and is waiting to run. Initial state on Create.</summary>
    Queued = 0,

    /// <summary>The training job is actively running.</summary>
    Running = 1,

    /// <summary>The training job completed successfully.</summary>
    Completed = 2,

    /// <summary>The training job failed.</summary>
    Failed = 3,

    /// <summary>The training job has been archived for compliance retention.</summary>
    Archived = 4,
}
