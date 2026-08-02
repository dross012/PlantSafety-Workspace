// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           IFinancialRecordDetailRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the FinancialRecord read-model.
//                  Implemented in the Infrastructure layer. Distinct namespace/name from both:
//                  (1) PlantSafety.Domain.Financials.Interfaces.IFinancialRecordRepository (the
//                  write-side aggregate repository), and (2) the pre-existing, unrelated
//                  PlantSafety.Application.Financials.Financials.Repositories.IFinancialsRepository
//                  (the untouched date-range read-only vertical). Named
//                  "IFinancialRecordDetailRepository" (not the "I<Name>Repository" convention used
//                  elsewhere in this Phase 4 pass) because "IFinancialsRepository" was already
//                  taken by the pre-existing sibling interface -- mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Repositories
//                  .IRCADetailRepository's identical naming precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Financials.Models;

namespace PlantSafety.Application.Financials.Repositories;

/// <summary>Read-only repository for FinancialRecord data. Implemented in the Infrastructure layer.</summary>
public interface IFinancialRecordDetailRepository
{
    /// <summary>
    /// Returns the FinancialRecord read-model record (including line items) for the given
    /// identity, scoped to the given tenant, or <see langword="null"/> if not found or if the
    /// record belongs to a different tenant.
    /// </summary>
    Task<FinancialRecordDetailDto?> GetByIdAsync(Guid financialRecordId, Guid tenantId, CancellationToken cancellationToken);
}
