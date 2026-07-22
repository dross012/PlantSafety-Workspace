// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Contractor Safety Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.ContractorSafety.Models;

namespace PlantSafety.Application.SafetyPolicies.ContractorSafety.Repositories;

/// <summary>Read-only repository for Contractor Safety Policies data. Implemented in the Infrastructure layer.</summary>
public interface IContractorSafetyRepository
{
    /// <summary>Returns Contractor Safety Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ContractorSafetyItemDto>> GetContractorSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}