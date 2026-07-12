// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Texting read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Texting.Models;

namespace PlantSafety.Application.Integrations.Texting.Repositories;

/// <summary>Read-only repository for Integrations.Texting data. Implemented in the Infrastructure layer.</summary>
public interface ITextingRepository
{
    /// <summary>Returns Integrations.Texting text message items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TextingItemDto>> GetTextMessagesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
