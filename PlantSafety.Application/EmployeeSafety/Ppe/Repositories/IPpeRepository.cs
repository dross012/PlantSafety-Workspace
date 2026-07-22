// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Ppe read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.Ppe.Models;

namespace PlantSafety.Application.EmployeeSafety.Ppe.Repositories;

/// <summary>Read-only repository for Ppe data. Implemented in the Infrastructure layer.</summary>
public interface IPpeRepository
{
    /// <summary>Returns Ppe items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PpeItemDto>> GetPpeItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}