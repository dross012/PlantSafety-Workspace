// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           IContractorVisitorRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the ContractorVisitor read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.ContractorVisitor.Repositories.IContractorVisitorRepository
//                  (the write-side aggregate repository) -- same simple name, different namespace.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContractorVisitor.Models;

namespace PlantSafety.Application.ContractorVisitor.Repositories;

/// <summary>Read-only repository for ContractorVisitor data. Implemented in the Infrastructure layer.</summary>
public interface IContractorVisitorRepository
{
    /// <summary>
    /// Returns the ContractorVisitor read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<ContractorVisitorItemDto?> GetByIdAsync(Guid contractorVisitorId, Guid tenantId, CancellationToken cancellationToken);
}
