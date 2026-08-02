// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           IPredictiveSafetyAlertDetailRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single PredictiveSafetyAlert record by
//                  identity, backing GetPredictiveSafetyAlertByIdQuery. Deliberately a new, distinct
//                  interface from the pre-existing
//                  PlantSafety.Application.Intelligence.PredictiveSafety.Repositories
//                  .IPredictiveSafetyRepository (the date-range list read-model behind
//                  GetPredictiveSafetyQuery/usp_PredictiveSafety_GetPredictions), which this pass
//                  leaves entirely untouched. Implemented in the Infrastructure layer by
//                  PredictiveSafetyAlertDetailDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

/// <summary>Read-only repository for a single PredictiveSafetyAlert record by identity. Implemented in the Infrastructure layer.</summary>
public interface IPredictiveSafetyAlertDetailRepository
{
    /// <summary>
    /// Returns the full PredictiveSafetyAlert read-model record for the given identity, scoped to
    /// the given tenant, or <see langword="null"/> if not found or if it belongs to a different
    /// tenant.
    /// </summary>
    Task<PredictiveSafetyAlertDetailDto?> GetByIdAsync(Guid predictiveSafetyAlertId, Guid tenantId, CancellationToken cancellationToken);
}
