// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Infrastructure
//  File:           ObservationItemsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Observation read-model backing
//                  IObservationRepository (the legacy SafetyEvents.Observation.Repositories
//                  interface). Named ObservationItemsDapperRepository (not
//                  ObservationDapperRepository) because the unrelated Features-tree
//                  read model already declares a class named ObservationDapperRepository
//                  in PlantSafety.Infrastructure.Features.SafetyEvents.Observation.Repositories,
//                  and both namespaces are pulled in via global usings, which produces
//                  an unqualified-name ambiguity (CS0104) at any call site that does not
//                  fully qualify the type. This name backs the legacy interface only.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.Observation.Models;
using PlantSafety.Application.SafetyEvents.Observation.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.Observation.Persistence;

/// <summary>
/// Dapper-backed repository for Observation read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ObservationItemsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ObservationItemsDapperRepository> logger)
    : IObservationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ObservationItemDto>> GetObservationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetObservations for Observation tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ObservationItemDto>(
            "[SafetyEvents].[usp_ObservationItems_GetObservations]", parameters, cancellationToken);
    }
}
