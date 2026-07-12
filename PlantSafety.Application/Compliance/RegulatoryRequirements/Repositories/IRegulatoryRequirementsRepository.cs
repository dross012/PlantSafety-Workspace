// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the RegulatoryRequirements read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryRequirements.Models;

namespace PlantSafety.Application.Compliance.RegulatoryRequirements.Repositories;

/// <summary>Read-only repository for RegulatoryRequirements data. Implemented in the Infrastructure layer.</summary>
public interface IRegulatoryRequirementsRepository
{
    /// <summary>Returns RegulatoryRequirements items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RegulatoryRequirementsItemDto>> GetRegulatoryRequirementsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
