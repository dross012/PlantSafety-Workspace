// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PermitExpiration read-model.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitExpiration.Models;

namespace PlantSafety.Application.PermitToWork.PermitExpiration.Repositories;

/// <summary>Read-only repository for PermitExpiration data. Implemented in the Infrastructure layer.</summary>
public interface IPermitExpirationRepository
{
    /// <summary>Returns PermitExpiration items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitExpirationItemDto>> GetPermitExpirationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}