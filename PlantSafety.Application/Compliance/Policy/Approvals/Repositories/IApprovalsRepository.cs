// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Approvals read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Approvals.Models;

namespace PlantSafety.Application.Compliance.Policy.Approvals.Repositories;

/// <summary>Read-only repository for Approvals data. Implemented in the Infrastructure layer.</summary>
public interface IApprovalsRepository
{
    /// <summary>Returns Approvals items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ApprovalsItemDto>> GetApprovalsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
