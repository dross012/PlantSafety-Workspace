// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           IComplianceEvaluationRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ComplianceEvaluation aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;
using PlantSafety.Domain.Compliance.RulesEngine.Specifications;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Interfaces;

/// <summary>Domain repository interface for the ComplianceEvaluation aggregate root.</summary>
public interface IComplianceEvaluationRepository
{
    /// <summary>Retrieves a ComplianceEvaluation aggregate by identity, scoped to the given tenant.</summary>
    Task<ComplianceEvaluation?> GetByIdAsync(ComplianceEvaluationId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all ComplianceEvaluation aggregates for the tenant matching the specification.</summary>
    Task<IReadOnlyList<ComplianceEvaluation>> ListAsync(Guid tenantId, ComplianceEvaluationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Returns the full evaluation history (most recent first) for a specific target entity.</summary>
    Task<IReadOnlyList<ComplianceEvaluation>> GetHistoryForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ComplianceEvaluation aggregate.</summary>
    Task AddAsync(ComplianceEvaluation aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ComplianceEvaluation aggregate.</summary>
    Task UpdateAsync(ComplianceEvaluation aggregate, CancellationToken cancellationToken = default);
}
