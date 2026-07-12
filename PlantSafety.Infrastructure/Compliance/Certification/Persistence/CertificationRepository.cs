// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Infrastructure
//  File:           CertificationRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Certification.Persistence;

/// <summary>Dapper-backed repository for <see cref="CertificationReadModel"/> queries.</summary>
public sealed class CertificationRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CertificationRepository> logger)
{
    /// <summary>Returns the Certification read model or <c>null</c> if not found.</summary>
    public async Task<CertificationReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Certification {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CertificationId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CertificationReadModel>(
            $"[Compliance].[usp_Certification_GetById]", parameters);
    }

    /// <summary>Returns all Certification records for the tenant.</summary>
    public async Task<IReadOnlyList<CertificationReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Certification tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<CertificationReadModel>(
            $"[Compliance].[usp_Certification_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
