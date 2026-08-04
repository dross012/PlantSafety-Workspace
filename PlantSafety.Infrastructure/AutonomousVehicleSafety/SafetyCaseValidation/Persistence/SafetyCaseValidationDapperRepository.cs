// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Infrastructure
//  File:           SafetyCaseValidationDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyCaseValidation aggregate. Implements both the
//                  Domain-layer write repository (ISafetyCaseValidationRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetSafetyCaseStatusByAssetIdAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Specifications;
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.ValueObjects.Ids;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Aggregates.SafetyCaseValidation;
using IWriteRepo = PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Interfaces.ISafetyCaseValidationRepository;
using IReadRepo = PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Repositories.ISafetyCaseValidationRepository;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.SafetyCaseValidation.Persistence;

/// <summary>Flat Dapper row for [AutonomousVehicleSafety].[SafetyCaseValidation], column-for-column with the SafetyCaseValidation aggregate.</summary>
internal sealed class SafetyCaseValidationRow
{
    public Guid SafetyCaseValidationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid AutonomousVehicleAssetId { get; init; }
    public string? StandardReference { get; init; }
    public string? DocumentVersion { get; init; }
    public int RevalidationCadenceDays { get; init; }
    public Guid? ValidatedByUserId { get; init; }
    public DateTime? ValidatedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the SafetyCaseValidation aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class SafetyCaseValidationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SafetyCaseValidationDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "AutonomousVehicleSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(SafetyCaseValidationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for SafetyCaseValidation {SafetyCaseValidationId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SafetyCaseValidationId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SafetyCaseValidationRow>(
            $"[{Schema}].[usp_SafetyCaseValidation_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, SafetyCaseValidationRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(SafetyCaseValidationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for SafetyCaseValidation");

        var rows = await queryExecutor.QueryAsync<SafetyCaseValidationRow>(
            $"[{Schema}].[usp_SafetyCaseValidation_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, SafetyCaseValidationRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for SafetyCaseValidation {SafetyCaseValidationId}", aggregate.SafetyCaseValidationId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("SafetyCaseValidationId", aggregate, "SafetyCaseValidationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("StandardReference", aggregate, "StandardReference")
            .AddAggregateProperty("DocumentVersion", aggregate, "DocumentVersion")
            .AddAggregateProperty("RevalidationCadenceDays", aggregate, "RevalidationCadenceDays")
            .AddAggregateProperty("ValidatedByUserId", aggregate, "ValidatedByUserId")
            .AddAggregateProperty("ValidatedAtUtc", aggregate, "ValidatedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RejectionReason", aggregate, "RejectionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SafetyCaseValidation_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for SafetyCaseValidation {SafetyCaseValidationId}", aggregate.SafetyCaseValidationId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("SafetyCaseValidationId", aggregate, "SafetyCaseValidationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("StandardReference", aggregate, "StandardReference")
            .AddAggregateProperty("DocumentVersion", aggregate, "DocumentVersion")
            .AddAggregateProperty("RevalidationCadenceDays", aggregate, "RevalidationCadenceDays")
            .AddAggregateProperty("ValidatedByUserId", aggregate, "ValidatedByUserId")
            .AddAggregateProperty("ValidatedAtUtc", aggregate, "ValidatedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RejectionReason", aggregate, "RejectionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SafetyCaseValidation_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetSafetyCaseStatusByAssetIdAsync"/>
    public async Task<SafetyCaseValidationItemDto?> GetSafetyCaseStatusByAssetIdAsync(
        Guid tenantId,
        Guid autonomousVehicleAssetId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSafetyCaseStatusByAssetIdAsync for Asset {AutonomousVehicleAssetId}, tenant {TenantId}", autonomousVehicleAssetId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AutonomousVehicleAssetId", autonomousVehicleAssetId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<SafetyCaseValidationItemDto>(
            $"[{Schema}].[usp_SafetyCaseValidation_GetStatusByAssetId]", parameters, cancellationToken);
    }
}
