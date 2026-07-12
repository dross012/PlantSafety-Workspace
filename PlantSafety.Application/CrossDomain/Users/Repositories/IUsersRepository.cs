// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Users read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Users.Models;

namespace PlantSafety.Application.CrossDomain.Users.Repositories;

/// <summary>Read-only repository for Users data. Implemented in the Infrastructure layer.</summary>
public interface IUsersRepository
{
    /// <summary>Returns Users items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<UsersItemDto>> GetUsersAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
