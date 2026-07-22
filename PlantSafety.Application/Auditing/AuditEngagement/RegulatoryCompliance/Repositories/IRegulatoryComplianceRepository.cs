// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the RegulatoryCompliance read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Repositories;

/// <summary>Read-only repository for RegulatoryCompliance data. Implemented in the Infrastructure layer.</summary>
public interface IRegulatoryComplianceRepository
{
    /// <summary>Returns RegulatoryCompliance items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RegulatoryComplianceItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}