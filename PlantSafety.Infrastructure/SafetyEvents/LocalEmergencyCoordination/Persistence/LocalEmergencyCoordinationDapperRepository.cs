// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Infrastructure
//  File:           LocalEmergencyCoordinationDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LocalEmergencyCoordination read-model backing the
//                  Application-layer ILocalEmergencyCoordinationRepository. Distinct from
//                  LocalEmergencyCoordinationWriteDapperRepository in this same folder, which backs
//                  the Domain-layer write-side aggregate repository. All DB access via stored
//                  procedures. Mirrors EmergencyManagementDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.LocalEmergencyCoordination.Persistence;

/// <summary>Dapper-backed repository for LocalEmergencyCoordination read-model queries. Executes stored procedures for all persistence operations.</summary>
public sealed class LocalEmergencyCoordinationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<LocalEmergencyCoordinationDapperRepository> logger)
    : ILocalEmergencyCoordinationRepository
{
    /// <inheritdoc/>
    public async Task<LocalEmergencyCoordinationItemDto?> GetByIdAsync(Guid localEmergencyCoordinationId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for LocalEmergencyCoordination {LocalEmergencyCoordinationId} Tenant {TenantId}", localEmergencyCoordinationId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LocalEmergencyCoordinationId", localEmergencyCoordinationId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<LocalEmergencyCoordinationItemDto>(
            "[SafetyEvents].[usp_LocalEmergencyCoordinationCases_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MutualAidAgreementItemDto>> ListMutualAidAgreementsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("ListMutualAidAgreements for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<MutualAidAgreementItemDto>(
            "[SafetyEvents].[usp_MutualAidAgreements_List]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<JointResponsePlanItemDto>> GetJointResponsePlansByIncidentAsync(Guid tenantId, Guid incidentId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetJointResponsePlansByIncident {IncidentId} Tenant {TenantId}", incidentId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .Build();

        return await queryExecutor.QueryAsync<JointResponsePlanItemDto>(
            "[SafetyEvents].[usp_JointResponsePlans_GetByIncident]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LiaisonLogEntryItemDto>> GetLiaisonLogByIncidentAsync(Guid tenantId, Guid incidentId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLiaisonLogByIncident {IncidentId} Tenant {TenantId}", incidentId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .Build();

        return await queryExecutor.QueryAsync<LiaisonLogEntryItemDto>(
            "[SafetyEvents].[usp_LiaisonLogEntries_GetByIncident]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<JointDebriefItemDto>> ListJointDebriefsByPeriodAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("ListJointDebriefsByPeriod for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<JointDebriefItemDto>(
            "[SafetyEvents].[usp_JointDebriefRecords_ListByPeriod]", parameters, cancellationToken);
    }
}
