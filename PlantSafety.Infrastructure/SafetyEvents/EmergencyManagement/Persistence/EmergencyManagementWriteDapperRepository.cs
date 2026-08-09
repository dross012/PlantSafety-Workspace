// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Infrastructure
//  File:           EmergencyManagementWriteDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer
//                  IEmergencyManagementRepository (write-side aggregate repository). Named
//                  *WriteDapperRepository (not EmergencyManagementDapperRepository, which already
//                  backs the read-only Application-layer IEmergencyManagementRepository against
//                  usp_EmergencyManagementItems_GetEmergencies) to avoid an unqualified-name
//                  collision between the two distinct IEmergencyManagementRepository interfaces
//                  (Domain vs Application namespaces). All DB access via stored procedures
//                  against [SafetyEvents].[EmergencyManagementItems] (the same table backing the
//                  read-only list query). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because EmergencyManagement exposes only a private
//                  "for rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied EmergencyManagementSpecification in-memory over the full
//                  active candidate set (usp_EmergencyManagementItems_GetAllActive), since the
//                  interface carries no tenant/site scoping parameter to push down to SQL --
//                  mirrors PermitsDapperRepository.ListAsync / usp_Permits_GetAllActive.sql. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write. GetByIdAsync
//                  is tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from
//                  day one. Mirrors PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Enums;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Specifications;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement"/>
/// aggregate root (Domain write-side repository interface).
/// </summary>
public sealed class EmergencyManagementWriteDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EmergencyManagementWriteDapperRepository> logger,
    ICrossModuleEventRelayV2? crossModuleEventRelayV2 = null)
    : IEmergencyManagementRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement?> GetByIdAsync(EmergencyManagementId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EmergencyManagement {EmergencyManagementId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EmergencyManagementRow>(
            "[SafetyEvents].[usp_EmergencyManagementItems_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement>> ListAsync(EmergencyManagementSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List EmergencyManagement via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<EmergencyManagementRow>(
            "[SafetyEvents].[usp_EmergencyManagementItems_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add EmergencyManagement {EmergencyManagementId}", aggregate.EmergencyManagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyId", aggregate.EmergencyManagementId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("EmergencyType", aggregate.EmergencyType, 50)
            .AddString("Severity", aggregate.Severity, 50)
            .AddString("Location", aggregate.Location, 400)
            .AddInt("CasualtyCount", aggregate.CasualtyCount)
            .AddBool("ResponseTeamNotified", aggregate.ResponseTeamNotified)
            .AddDateTime("ReportedAtUtc", aggregate.DeclaredAtUtc.Value)
            .AddDateTime("StabilizedAtUtc", aggregate.StabilizedAtUtc?.Value)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddGuid("ReportedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_EmergencyManagementItems_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring: enqueue any domain events raised by the aggregate during this write so
        // TransactionalUnitOfWork.CommitAsync (invoked by TransactionBehavior after the handler
        // returns) can flush them to [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
            // Phase B.3 (Path B): first-ever cross-module relay for EmergencyManagement -- see
            // EmergencyManagementCreatedIntegrationEventV2.cs's header. V1 has none, so only the
            // V2 relay is wired here.
            crossModuleEventRelayV2?.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();

        if (crossModuleEventRelayV2 is { HasPendingEvents: true })
            await crossModuleEventRelayV2.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update EmergencyManagement {EmergencyManagementId}", aggregate.EmergencyManagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyId", aggregate.EmergencyManagementId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("EmergencyType", aggregate.EmergencyType, 50)
            .AddString("Severity", aggregate.Severity, 50)
            .AddString("Location", aggregate.Location, 400)
            .AddInt("CasualtyCount", aggregate.CasualtyCount)
            .AddBool("ResponseTeamNotified", aggregate.ResponseTeamNotified)
            .AddDateTime("StabilizedAtUtc", aggregate.StabilizedAtUtc?.Value)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_EmergencyManagementItems_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_EmergencyManagementItems_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"EmergencyManagement {aggregate.EmergencyManagementId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring: UpdateAsync must enqueue domain events too, not just AddAsync --
        // otherwise every post-creation transition (RespondTo, Stabilize, Resolve, Close,
        // Archive) would silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
            crossModuleEventRelayV2?.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();

        if (crossModuleEventRelayV2 is { HasPendingEvents: true })
            await crossModuleEventRelayV2.FlushAsync(cancellationToken);
    }

    /// <summary>Reconstructs an <see cref="PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement Hydrate(EmergencyManagementRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.EmergencyManagementId), EmergencyManagementId.FromGuid(row.EmergencyId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.Status), Enum.Parse<EmergencyManagementStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.EmergencyType), row.EmergencyType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.Severity), row.Severity);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.CasualtyCount), row.CasualtyCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.ResponseTeamNotified), row.ResponseTeamNotified);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.DeclaredAtUtc), UtcTimestamp.From(row.ReportedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.StabilizedAtUtc), row.StabilizedAtUtc.HasValue ? UtcTimestamp.From(row.StabilizedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.ResolvedAtUtc), row.ResolvedAtUtc.HasValue ? UtcTimestamp.From(row.ResolvedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.CreatedAtUtc), UtcTimestamp.From(row.ReportedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.CreatedBy), new UserId(row.ReportedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IEmergencyManagementDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_EmergencyManagementItems_GetById</c>/<c>usp_EmergencyManagementItems_GetEmergencies</c>.</summary>
    private sealed record EmergencyManagementRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_EmergencyManagementItems_GetById</c>. Not consumed by aggregate hydration --
        /// callers obtain the ETag for a subsequent write from a prior single-record read.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid EmergencyId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string EmergencyType { get; init; }
        public required string Severity { get; init; }
        public string? Location { get; init; }
        public int CasualtyCount { get; init; }
        public bool ResponseTeamNotified { get; init; }
        public required DateTime ReportedAtUtc { get; init; }
        public DateTime? StabilizedAtUtc { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public required Guid ReportedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
