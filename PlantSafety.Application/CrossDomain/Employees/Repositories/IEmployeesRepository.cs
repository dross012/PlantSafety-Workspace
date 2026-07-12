// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Employees read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Employees.Models;

namespace PlantSafety.Application.CrossDomain.Employees.Repositories;

/// <summary>Read-only repository for Employees data. Implemented in the Infrastructure layer.</summary>
public interface IEmployeesRepository
{
    /// <summary>Returns Employees items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmployeesItemDto>> GetEmployeesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
