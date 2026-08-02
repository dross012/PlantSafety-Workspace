// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DigitalTwin Application-layer read-model. Backs
//                  GetDigitalTwinByIdQuery. Reuses usp_DigitalTwinAggregate_GetById. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.DigitalTwin.Models;
using PlantSafety.Application.DigitalTwin.Repositories;

namespace PlantSafety.Infrastructure.DigitalTwin.Persistence;

/// <summary>Dapper-backed repository for DigitalTwin read-model queries.</summary>
public sealed class DigitalTwinReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<DigitalTwinReadDapperRepository> logger)
    : IDigitalTwinRepository
{
    /// <inheritdoc/>
    public async Task<DigitalTwinItemDto?> GetByIdAsync(Guid digitalTwinId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for DigitalTwin {DigitalTwinId} Tenant {TenantId}", digitalTwinId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DigitalTwinId", digitalTwinId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DigitalTwinItemDto>(
            "[DigitalTwin].[usp_DigitalTwinAggregate_GetById]", parameters, cancellationToken);
    }
}
