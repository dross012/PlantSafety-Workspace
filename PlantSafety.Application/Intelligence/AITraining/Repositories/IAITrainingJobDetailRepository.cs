// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           IAITrainingJobDetailRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single AITrainingJob record by identity,
//                  backing GetAITrainingJobByIdQuery. Deliberately a new, distinct interface from
//                  the pre-existing PlantSafety.Application.Intelligence.AITraining.Repositories
//                  .IAITrainingRepository (the date-range list read-model behind
//                  GetAITrainingQuery/usp_AITraining_GetTrainingJobs), which this pass leaves
//                  entirely untouched. Implemented in the Infrastructure layer by
//                  AITrainingJobDetailDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AITraining.Models;

namespace PlantSafety.Application.Intelligence.AITraining.Repositories;

/// <summary>Read-only repository for a single AITrainingJob record by identity. Implemented in the Infrastructure layer.</summary>
public interface IAITrainingJobDetailRepository
{
    /// <summary>
    /// Returns the full AITrainingJob read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<AITrainingJobDetailDto?> GetByIdAsync(Guid aiTrainingJobId, Guid tenantId, CancellationToken cancellationToken);
}
