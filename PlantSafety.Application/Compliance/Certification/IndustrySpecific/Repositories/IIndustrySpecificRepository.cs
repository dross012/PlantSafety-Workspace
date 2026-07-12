// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the IndustrySpecific read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Models;

namespace PlantSafety.Application.Compliance.Certification.IndustrySpecific.Repositories;

/// <summary>Read-only repository for IndustrySpecific data. Implemented in the Infrastructure layer.</summary>
public interface IIndustrySpecificRepository
{
    /// <summary>Returns IndustrySpecific items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IndustrySpecificItemDto>> GetIndustrySpecificAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
