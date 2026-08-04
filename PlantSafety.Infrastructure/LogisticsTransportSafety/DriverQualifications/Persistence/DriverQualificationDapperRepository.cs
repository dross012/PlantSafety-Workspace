// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Infrastructure
//  File:           DriverQualificationDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DriverQualification aggregate. Implements both the
//                  Domain-layer write repository (IDriverQualificationRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetDriverQualificationStatusAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  DriverQualification exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Specifications;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.ValueObjects.Ids;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification;
using IWriteRepo = PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces.IDriverQualificationRepository;
using IReadRepo = PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Repositories.IDriverQualificationRepository;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.DriverQualifications.Persistence;

/// <summary>Flat Dapper row for [LogisticsTransportSafety].[DriverQualifications], column-for-column with the DriverQualification aggregate.</summary>
internal sealed class DriverQualificationRow
{
    public Guid DriverQualificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid DriverUserId { get; init; }
    public string? QualificationType { get; init; }
    public string? LicenseNumber { get; init; }
    public DateTime? IssuedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public string? SuspensionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the DriverQualification aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class DriverQualificationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DriverQualificationDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "LogisticsTransportSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(DriverQualificationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for DriverQualification {DriverQualificationId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DriverQualificationId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<DriverQualificationRow>(
            $"[{Schema}].[usp_DriverQualification_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, DriverQualificationRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(DriverQualificationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for DriverQualification");

        var rows = await queryExecutor.QueryAsync<DriverQualificationRow>(
            $"[{Schema}].[usp_DriverQualification_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, DriverQualificationRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for DriverQualification {DriverQualificationId}", aggregate.DriverQualificationId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("DriverQualificationId", aggregate, "DriverQualificationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DriverUserId", aggregate, "DriverUserId")
            .AddAggregateProperty("QualificationType", aggregate, "QualificationType")
            .AddAggregateProperty("LicenseNumber", aggregate, "LicenseNumber")
            .AddAggregateProperty("IssuedAtUtc", aggregate, "IssuedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DriverQualification_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for DriverQualification {DriverQualificationId}", aggregate.DriverQualificationId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("DriverQualificationId", aggregate, "DriverQualificationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DriverQualification_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetDriverQualificationStatusAsync"/>
    public async Task<IReadOnlyList<DriverQualificationItemDto>> GetDriverQualificationStatusAsync(
        Guid tenantId,
        Guid driverUserId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDriverQualificationStatusAsync for driver {DriverUserId}", driverUserId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("DriverUserId", driverUserId)
            .Build();

        return await queryExecutor.QueryAsync<DriverQualificationItemDto>(
            $"[{Schema}].[usp_DriverQualification_GetStatusByDriver]", parameters, cancellationToken);
    }
}
