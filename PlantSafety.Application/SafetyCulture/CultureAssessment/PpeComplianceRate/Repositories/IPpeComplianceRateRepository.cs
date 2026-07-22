// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PpeComplianceRate read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Repositories;

/// <summary>Read-only repository for PpeComplianceRate data. Implemented in the Infrastructure layer.</summary>
public interface IPpeComplianceRateRepository
{
    /// <summary>Returns PpeComplianceRate items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PpeComplianceRateItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}