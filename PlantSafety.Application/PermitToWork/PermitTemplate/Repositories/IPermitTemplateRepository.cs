// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PermitTemplate read-model.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitTemplate.Models;

namespace PlantSafety.Application.PermitToWork.PermitTemplate.Repositories;

/// <summary>Read-only repository for PermitTemplate data. Implemented in the Infrastructure layer.</summary>
public interface IPermitTemplateRepository
{
    /// <summary>Returns PermitTemplate items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitTemplateItemDto>> GetPermitTemplateAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}