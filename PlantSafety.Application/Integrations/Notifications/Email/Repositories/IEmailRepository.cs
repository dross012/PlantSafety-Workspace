// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Email read-model.
// SchemaVersion: 1
using PlantSafety.Application.Integrations.Notifications.Email.Models;

namespace PlantSafety.Application.Integrations.Notifications.Email.Repositories;

/// <summary>Read-only repository for Email data. Implemented in the Infrastructure layer.</summary>
public interface IEmailRepository
{
    /// <summary>Returns Email items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmailItemDto>> GetEmailDeliveriesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}