// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PpePolicies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.PpePolicies.Models;

namespace PlantSafety.Application.SafetyProcedures.PpePolicies.Repositories;

/// <summary>Read-only repository for PpePolicies data. Implemented in the Infrastructure layer.</summary>
public interface IPpePoliciesRepository
{
    /// <summary>Returns PpePolicies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PpePoliciesItemDto>> GetPpePoliciesItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}