// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the CAPATriggers read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.CAPATriggers.Models;

namespace PlantSafety.Application.Compliance.Auditing.CAPATriggers.Repositories;

/// <summary>Read-only repository for CAPATriggers data. Implemented in the Infrastructure layer.</summary>
public interface ICAPATriggersRepository
{
    /// <summary>Returns CAPATriggers items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CAPATriggerItemDto>> GetTriggersAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
