// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Operators read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Operators.Models;

namespace PlantSafety.Application.CrossDomain.Operators.Repositories;

/// <summary>Read-only repository for Operators data. Implemented in the Infrastructure layer.</summary>
public interface IOperatorsRepository
{
    /// <summary>Returns Operators items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<OperatorsItemDto>> GetOperatorsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
