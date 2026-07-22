// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Training & Contractor Management read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Models;

namespace PlantSafety.Application.RiskManagement.TrainingContractorManagement.Repositories;

/// <summary>Read-only repository for Training & Contractor Management data. Implemented in the Infrastructure layer.</summary>
public interface ITrainingContractorManagementRepository
{
    /// <summary>Returns Training & Contractor Management items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TrainingContractorManagementItemDto>> GetTrainingContractorManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}