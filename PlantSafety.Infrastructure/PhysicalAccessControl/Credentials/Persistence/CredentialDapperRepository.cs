// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Infrastructure
//  File:           CredentialDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Credential aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository. Badge/credential PII is masked at the SQL layer for the
//                  read-model per the module's data-governance policy.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Specifications;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential;
using IWriteRepo = PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces.ICredentialRepository;
using IReadRepo = PlantSafety.Application.PhysicalAccessControl.Credentials.Repositories.ICredentialReadRepository;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.Credentials.Persistence;

/// <summary>Flat Dapper row for [PhysicalAccessControl].[Credentials], column-for-column with the Credential aggregate.</summary>
internal sealed class CredentialRow
{
    public Guid CredentialId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? BadgeNumber { get; init; }
    public Guid HolderUserId { get; init; }
    public string? HolderType { get; init; }
    public string? AccessLevel { get; init; }
    public DateTime? IssuedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public string? RevocationReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the Credential aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CredentialDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CredentialDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "PhysicalAccessControl";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Credential {CredentialId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CredentialId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CredentialRow>(
            $"[{Schema}].[usp_Credentials_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CredentialRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(CredentialSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for Credential");

        var rows = await queryExecutor.QueryAsync<CredentialRow>(
            $"[{Schema}].[usp_Credentials_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, CredentialRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for Credential {CredentialId}", aggregate.CredentialId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CredentialId", aggregate, "CredentialId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("BadgeNumber", aggregate, "BadgeNumber")
            .AddAggregateProperty("HolderUserId", aggregate, "HolderUserId")
            .AddAggregateProperty("HolderType", aggregate, "HolderType")
            .AddAggregateProperty("AccessLevel", aggregate, "AccessLevel")
            .AddAggregateProperty("IssuedAtUtc", aggregate, "IssuedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Credentials_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for Credential {CredentialId}", aggregate.CredentialId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CredentialId", aggregate, "CredentialId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("BadgeNumber", aggregate, "BadgeNumber")
            .AddAggregateProperty("HolderUserId", aggregate, "HolderUserId")
            .AddAggregateProperty("HolderType", aggregate, "HolderType")
            .AddAggregateProperty("AccessLevel", aggregate, "AccessLevel")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RevocationReason", aggregate, "RevocationReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Credentials_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetStatusAsync"/>
    public async Task<CredentialStatusDto?> GetStatusAsync(Guid tenantId, Guid credentialId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetStatusAsync (read-model) for Credential {CredentialId}", credentialId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CredentialId", credentialId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CredentialStatusDto>(
            $"[{Schema}].[usp_Credentials_GetStatusItem]", parameters, cancellationToken);
    }
}
