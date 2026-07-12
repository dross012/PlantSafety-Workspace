// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HR read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.HR.Models;

namespace PlantSafety.Application.Compliance.Policy.HR.Repositories;

/// <summary>Read-only repository for HR data. Implemented in the Infrastructure layer.</summary>
public interface IHRRepository
{
    /// <summary>Returns HR items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HRItemDto>> GetHRPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
