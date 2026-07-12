// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PPE read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.PPE.Models;

namespace PlantSafety.Application.Compliance.Policy.PPE.Repositories;

/// <summary>Read-only repository for PPE data. Implemented in the Infrastructure layer.</summary>
public interface IPPERepository
{
    /// <summary>Returns PPE items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPEItemDto>> GetPPEPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
