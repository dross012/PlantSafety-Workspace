// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the FacilityAssessment read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Repositories;

/// <summary>Read-only repository for FacilityAssessment data. Implemented in the Infrastructure layer.</summary>
public interface IFacilityAssessmentRepository
{
    /// <summary>Returns FacilityAssessment items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<FacilityAssessmentItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}