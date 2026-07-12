// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Email read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Email.Models;

namespace PlantSafety.Application.Integrations.Email.Repositories;

/// <summary>Read-only repository for Integrations.Email data. Implemented in the Infrastructure layer.</summary>
public interface IEmailRepository
{
    /// <summary>Returns Integrations.Email items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmailItemDto>> GetEmailsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
