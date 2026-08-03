// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the
//   ComplianceEvaluation read model, mirroring the RiskManagement precedent of a separate
//   Application-facing read repository alongside the Domain-facing write repository.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RulesEngine.Models;

namespace PlantSafety.Application.Compliance.RulesEngine.Repositories;

/// <summary>Read-only repository for the ComplianceEvaluation read model. Implemented in Infrastructure.</summary>
public interface IComplianceEvaluationReadRepository
{
    /// <summary>Returns a single evaluation by id, scoped to the tenant, or null.</summary>
    Task<ComplianceEvaluationDto?> GetByIdAsync(Guid tenantId, Guid complianceEvaluationId, CancellationToken cancellationToken);

    /// <summary>Returns the evaluation history for a target entity, most recent first.</summary>
    Task<IReadOnlyList<ComplianceEvaluationDto>> GetHistoryForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken);
}
