// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Texting read-model.
// SchemaVersion: 1
using PlantSafety.Application.Integrations.Notifications.Texting.Models;

namespace PlantSafety.Application.Integrations.Notifications.Texting.Repositories;

/// <summary>Read-only repository for Texting data. Implemented in the Infrastructure layer.</summary>
public interface ITextingRepository
{
    /// <summary>Returns Texting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TextingItemDto>> GetTextingDeliveriesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}