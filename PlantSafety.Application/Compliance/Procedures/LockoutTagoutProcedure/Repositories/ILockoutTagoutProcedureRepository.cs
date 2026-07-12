// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the LockoutTagoutProcedure read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Models;

namespace PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Repositories;

/// <summary>Read-only repository for LockoutTagoutProcedure data. Implemented in the Infrastructure layer.</summary>
public interface ILockoutTagoutProcedureRepository
{
    /// <summary>Returns LockoutTagoutProcedure items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LockoutTagoutProcedureItemDto>> GetProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
