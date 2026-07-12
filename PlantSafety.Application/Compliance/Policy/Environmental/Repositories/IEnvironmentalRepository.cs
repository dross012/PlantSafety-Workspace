// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Environmental read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Environmental.Models;

namespace PlantSafety.Application.Compliance.Policy.Environmental.Repositories;

/// <summary>Read-only repository for Environmental data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalRepository
{
    /// <summary>Returns Environmental items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EnvironmentalItemDto>> GetEnvironmentalPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
