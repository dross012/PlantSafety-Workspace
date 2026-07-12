// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the SafetyPolicies read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Models;

namespace PlantSafety.Application.Compliance.Policy.SafetyPolicies.Repositories;

/// <summary>Read-only repository for SafetyPolicies data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyPoliciesRepository
{
    /// <summary>Returns SafetyPolicies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetyPoliciesItemDto>> GetSafetyPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
