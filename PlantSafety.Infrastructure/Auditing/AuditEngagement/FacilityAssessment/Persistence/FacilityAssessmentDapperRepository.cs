// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.FacilityAssessment
//  Layer:          Infrastructure
//  File:           FacilityAssessmentDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the FacilityAssessment read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Models;
using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.FacilityAssessment.Persistence;

/// <summary>Dapper-backed repository for FacilityAssessment read-model queries.</summary>
public sealed class FacilityAssessmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<FacilityAssessmentDapperRepository> logger)
    : IFacilityAssessmentRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FacilityAssessmentItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for FacilityAssessment tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<FacilityAssessmentItemDto>(
            "[Auditing].[usp_FacilityAssessment_GetItems]", parameters, cancellationToken);
    }
}
