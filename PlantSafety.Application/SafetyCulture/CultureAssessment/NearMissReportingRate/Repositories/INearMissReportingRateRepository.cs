// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the NearMissReportingRate read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Repositories;

/// <summary>Read-only repository for NearMissReportingRate data. Implemented in the Infrastructure layer.</summary>
public interface INearMissReportingRateRepository
{
    /// <summary>Returns NearMissReportingRate items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<NearMissReportingRateItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}