// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Infrastructure
//  File:           IncidentReportReadDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the IncidentReport Application-layer read-model. Backs
//                  GetIncidentReportQuery and ListIncidentReportsQuery. Reuses
//                  usp_PHMSAIncidentReport_GetById (the same proc IncidentReportDapperRepository.
//                  GetByIdAsync uses to hydrate the write-side aggregate) and
//                  usp_PHMSAIncidentReport_GetAllActive rather than introducing duplicate stored
//                  procedures. ListAsync pushes @FromUtc/@ToUtc/@IsReportableToPhmsa down to SQL
//                  and then filters by tenant/site in-memory, since
//                  usp_PHMSAIncidentReport_GetAllActive carries no @TenantId/@SiteId parameters
//                  (mirrors PermitsReadDapperRepository / IncidentReportDapperRepository.ListAsync's
//                  identical in-memory tenant-filtering precedent).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Incidents.Models;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Repositories;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Incidents.Persistence;

/// <summary>Dapper-backed repository for IncidentReport read-model queries.</summary>
public sealed class IncidentReportReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentReportReadDapperRepository> logger)
    : IIncidentReportRepository
{
    /// <inheritdoc/>
    public async Task<IncidentReportDto?> GetByIdAsync(Guid incidentReportId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for IncidentReport {IncidentReportId} Tenant {TenantId}", incidentReportId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentReportId", incidentReportId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<IncidentReportDto>(
            "[Compliance].[usp_PHMSAIncidentReport_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentReportDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime? fromUtc,
        DateTime? toUtc,
        bool? isReportableToPhmsa,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List read-model for IncidentReport Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .AddBool("IsReportableToPhmsa", isReportableToPhmsa)
            .Build();

        var rows = await queryExecutor.QueryAsync<IncidentReportDto>(
            "[Compliance].[usp_PHMSAIncidentReport_GetAllActive]", parameters, cancellationToken);

        return rows
            .Where(row => row.TenantId == tenantId && (siteId is null || row.SiteId == siteId))
            .ToList();
    }
}
