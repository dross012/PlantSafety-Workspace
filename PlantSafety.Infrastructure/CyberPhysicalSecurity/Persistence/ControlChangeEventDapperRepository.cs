// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity
//  Layer:          Infrastructure
//  File:           ControlChangeEventDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the ControlChangeEvent aggregate root. Backs the
//                  tamper-evident, append-only, hash-chained OT control-change log: AddAsync only
//                  inserts (never updates the hash-bearing columns); UpdateAsync is restricted to
//                  the AuthorizationStatus reclassification column. Implements both the Domain
//                  write-side repository and the Application read-side repository.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Repositories;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Aggregates;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;

namespace PlantSafety.Infrastructure.CyberPhysicalSecurity.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ControlChangeEvent"/> aggregate root.</summary>
public sealed class ControlChangeEventDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ControlChangeEventDapperRepository> logger)
    : IControlChangeEventRepository, IControlChangeEventReadRepository
{
    /// <inheritdoc/>
    public async Task<ControlChangeEvent?> GetByIdAsync(ControlChangeEventId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("ControlChangeEventId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ControlChangeEventRow>(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<ControlChangeEvent?> GetLatestForAssetAsync(OtAssetId otAssetId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("OtAssetId", otAssetId.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ControlChangeEventRow>(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetLatestForAsset]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ControlChangeEvent>> ListByAuthorizationStatusAsync(Guid tenantId, ControlChangeAuthorizationStatus status, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddString("AuthorizationStatus", status.ToString(), 20).Build();
        var rows = await queryExecutor.QueryAsync<ControlChangeEventRow>(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetByAuthorizationStatus]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    async Task<IReadOnlyList<ControlChangeEventDto>> IControlChangeEventReadRepository.ListUnauthorizedAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddString("AuthorizationStatus", ControlChangeAuthorizationStatus.Unauthorized.ToString(), 20).Build();
        var rows = await queryExecutor.QueryAsync<ControlChangeEventRow>(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetByAuthorizationStatus]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(ControlChangeEvent aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ControlChangeEvent {ControlChangeEventId}", aggregate.ControlChangeEventId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ControlChangeEventId", aggregate.ControlChangeEventId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("OtAssetId", aggregate.OtAssetId.Value)
            .AddString("ChangeDescription", aggregate.ChangeDescription, 2000)
            .AddString("ChangedParameter", aggregate.ChangedParameter, 256)
            .AddString("OldValue", aggregate.OldValue, 512)
            .AddString("NewValue", aggregate.NewValue, 512)
            .AddGuid("ChangeSourceUserId", aggregate.ChangeSourceUserId?.Value)
            .AddString("AuthorizationStatus", aggregate.AuthorizationStatus.ToString(), 20)
            .AddDateTime("DetectedAtUtc", aggregate.DetectedAtUtc.Value)
            .AddString("PriorRecordHash", aggregate.PriorRecordHash, 64)
            .AddString("RecordHash", aggregate.RecordHash, 64)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ControlChangeEvent aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update (reclassify) ControlChangeEvent {ControlChangeEventId}", aggregate.ControlChangeEventId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ControlChangeEventId", aggregate.ControlChangeEventId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("AuthorizationStatus", aggregate.AuthorizationStatus.ToString(), 20)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_ControlChangeEvent_ReclassifyAuthorization]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    private void FlushDomainEvents(ControlChangeEvent aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());

        aggregate.ClearDomainEvents();
    }

    private static ControlChangeEvent Hydrate(ControlChangeEventRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ControlChangeEvent>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.ControlChangeEventId), ControlChangeEventId.FromGuid(row.ControlChangeEventId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.OtAssetId), OtAssetId.FromGuid(row.OtAssetId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.ChangeDescription), row.ChangeDescription);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.ChangedParameter), row.ChangedParameter);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.OldValue), row.OldValue);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.NewValue), row.NewValue);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.ChangeSourceUserId), row.ChangeSourceUserId.HasValue ? new UserId(row.ChangeSourceUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.AuthorizationStatus), Enum.Parse<ControlChangeAuthorizationStatus>(row.AuthorizationStatus));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.DetectedAtUtc), UtcTimestamp.From(row.DetectedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.PriorRecordHash), row.PriorRecordHash);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.RecordHash), row.RecordHash);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ControlChangeEvent.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IControlChangeEventDomainEvent>());

        return agg;
    }

    private static ControlChangeEventDto ToDto(ControlChangeEventRow row) => new(
        row.ControlChangeEventId, row.TenantId, row.SiteId, row.OtAssetId, row.ChangeDescription,
        row.ChangedParameter, row.OldValue, row.NewValue, row.ChangeSourceUserId, row.AuthorizationStatus,
        row.DetectedAtUtc, row.RecordHash);

    /// <summary>Flat read-model row shape returned by the ControlChangeEvent stored procedures.</summary>
    private sealed record ControlChangeEventRow
    {
        public required Guid ControlChangeEventId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SiteId { get; init; }
        public required Guid OtAssetId { get; init; }
        public required string ChangeDescription { get; init; }
        public required string ChangedParameter { get; init; }
        public required string OldValue { get; init; }
        public required string NewValue { get; init; }
        public Guid? ChangeSourceUserId { get; init; }
        public required string AuthorizationStatus { get; init; }
        public required DateTime DetectedAtUtc { get; init; }
        public string? PriorRecordHash { get; init; }
        public required string RecordHash { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
