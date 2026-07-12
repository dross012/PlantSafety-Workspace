// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Revisions read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Revisions.Models;

namespace PlantSafety.Application.Compliance.Policy.Revisions.Repositories;

/// <summary>Read-only repository for Revisions data. Implemented in the Infrastructure layer.</summary>
public interface IRevisionsRepository
{
    /// <summary>Returns Revisions items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RevisionsItemDto>> GetRevisionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
