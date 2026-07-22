// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PreQualificationVetting read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.PreQualificationVetting.Models;

namespace PlantSafety.Application.ContractorSafety.PreQualificationVetting.Repositories;

/// <summary>Read-only repository for PreQualificationVetting data. Implemented in the Infrastructure layer.</summary>
public interface IPreQualificationVettingRepository
{
    /// <summary>Returns PreQualificationVetting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PreQualificationVettingItemDto>> GetPreQualificationVettingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}