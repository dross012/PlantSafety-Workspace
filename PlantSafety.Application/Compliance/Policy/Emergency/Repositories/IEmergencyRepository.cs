// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Emergency read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Emergency.Models;

namespace PlantSafety.Application.Compliance.Policy.Emergency.Repositories;

/// <summary>Read-only repository for Emergency data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyRepository
{
    /// <summary>Returns Emergency items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyItemDto>> GetEmergencyPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
