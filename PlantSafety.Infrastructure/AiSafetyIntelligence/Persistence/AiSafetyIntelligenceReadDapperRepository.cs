// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Infrastructure
//  File:           AiSafetyIntelligenceReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AiSafetyIntelligence Application-layer read-model.
//                  Backs GetAiSafetyIntelligenceByIdQuery. Reuses
//                  usp_AiSafetyIntelligenceAggregate_GetById. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AiSafetyIntelligence.Models;
using PlantSafety.Application.AiSafetyIntelligence.Repositories;

namespace PlantSafety.Infrastructure.AiSafetyIntelligence.Persistence;

/// <summary>Dapper-backed repository for AiSafetyIntelligence read-model queries.</summary>
public sealed class AiSafetyIntelligenceReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AiSafetyIntelligenceReadDapperRepository> logger)
    : IAiSafetyIntelligenceRepository
{
    /// <inheritdoc/>
    public async Task<AiSafetyIntelligenceItemDto?> GetByIdAsync(Guid aiSafetyIntelligenceId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for AiSafetyIntelligence {AiSafetyIntelligenceId} Tenant {TenantId}", aiSafetyIntelligenceId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiSafetyIntelligenceId", aiSafetyIntelligenceId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AiSafetyIntelligenceItemDto>(
            "[AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_GetById]", parameters, cancellationToken);
    }
}
