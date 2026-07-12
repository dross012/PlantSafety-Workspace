// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the DOT read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.DOT.Models;

namespace PlantSafety.Application.Compliance.Certification.DOT.Repositories;

/// <summary>Read-only repository for DOT data. Implemented in the Infrastructure layer.</summary>
public interface IDOTRepository
{
    /// <summary>Returns DOT items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DOTItemDto>> GetDOTAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
