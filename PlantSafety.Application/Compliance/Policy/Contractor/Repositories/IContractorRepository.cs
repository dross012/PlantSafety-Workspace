// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Contractor read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Contractor.Models;

namespace PlantSafety.Application.Compliance.Policy.Contractor.Repositories;

/// <summary>Read-only repository for Contractor data. Implemented in the Infrastructure layer.</summary>
public interface IContractorRepository
{
    /// <summary>Returns Contractor items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ContractorItemDto>> GetContractorPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
