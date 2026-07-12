// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.IoT read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.IoT.Models;

namespace PlantSafety.Application.Integrations.IoT.Repositories;

/// <summary>Read-only repository for Integrations.IoT data. Implemented in the Infrastructure layer.</summary>
public interface IIoTRepository
{
    /// <summary>Returns Integrations.IoT IoT reading items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IoTItemDto>> GetIoTReadingsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
