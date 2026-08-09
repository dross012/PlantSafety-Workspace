// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Infrastructure
//  File:           AccidentDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAccidentRepository (Domain repository
//                  interface for the Accident aggregate root). All DB access via stored
//                  procedures in the [AccidentManagement] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Accident exposes only a private "for
//                  rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied AccidentSpecification in-memory over the full active
//                  candidate set, since the Domain-layer specification is an
//                  Expression&lt;Func&lt;Accident,bool&gt;&gt; rather than a SQL predicate, and the
//                  interface carries no tenant/site scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Entities;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Domain.AccidentManagement.Specifications;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.AccidentManagement.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Accident"/> aggregate root.</summary>
public sealed class AccidentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AccidentDapperRepository> logger,
    ICrossModuleEventRelay? crossModuleEventRelay = null,
    ICrossModuleEventRelayV2? crossModuleEventRelayV2 = null)
    : IAccidentRepository
{
    /// <inheritdoc/>
    public async Task<Accident?> GetByIdAsync(AccidentId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Accident {AccidentId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccidentId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AccidentRow>(
            "[AccidentManagement].[usp_Accident_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var caRows = await GetCorrectiveActionRowsAsync(id, tenantId, cancellationToken);
        return Hydrate(row, caRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Accident>> ListAsync(AccidentSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Accident via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<AccidentRow>(
            "[AccidentManagement].[usp_Accident_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<Accident>();

        foreach (var row in rows)
        {
            var caRows = await GetCorrectiveActionRowsAsync(AccidentId.FromGuid(row.AccidentId), row.TenantId, cancellationToken);
            var aggregate = Hydrate(row, caRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(Accident aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Accident {AccidentId}", aggregate.AccidentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccidentId", aggregate.AccidentId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("AccidentTitle", aggregate.AccidentTitle, 400)
            .AddString("Location", aggregate.Location, 400)
            .AddString("Severity", aggregate.Severity.Code, 30)
            .AddInt("InjuredPersonCount", aggregate.InjuredPersonCount)
            .AddDateTime("OccurredAtUtc", aggregate.OccurredAtUtc.Value)
            .AddString("Category", aggregate.Category.Code, 100)
            .AddBool("SiteSecuredAndTriaged", aggregate.SiteSecuredAndTriaged)
            .AddGuid("InvestigatorId", aggregate.InvestigatorId?.Value)
            .AddDateTime("InvestigatorAssignedAtUtc", aggregate.InvestigatorAssignedAtUtc?.Value)
            .AddString("RootCause", aggregate.RootCause?.Code, 30)
            .AddString("RootCauseNotes", aggregate.RootCauseNotes, 4000)
            .AddDateTime("RootCauseRecordedAtUtc", aggregate.RootCauseRecordedAtUtc?.Value)
            .AddBool("RegulatoryFilingRequired", aggregate.RegulatoryFilingRequired)
            .AddString("RegulatoryAgency", aggregate.RegulatoryAgency, 200)
            .AddString("RegulatoryReferenceNumber", aggregate.RegulatoryReferenceNumber, 100)
            .AddDateTime("RegulatoryFiledAtUtc", aggregate.RegulatoryFiledAtUtc?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddBool("ReviewRequired", aggregate.ReviewRequired)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[AccidentManagement].[usp_Accident_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        await RelayDomainEventsAsync(aggregate, cancellationToken);
}

    /// <inheritdoc/>
    public async Task UpdateAsync(Accident aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Accident {AccidentId}", aggregate.AccidentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccidentId", aggregate.AccidentId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("Category", aggregate.Category.Code, 100)
            .AddBool("SiteSecuredAndTriaged", aggregate.SiteSecuredAndTriaged)
            .AddGuid("InvestigatorId", aggregate.InvestigatorId?.Value)
            .AddDateTime("InvestigatorAssignedAtUtc", aggregate.InvestigatorAssignedAtUtc?.Value)
            .AddString("RootCause", aggregate.RootCause?.Code, 30)
            .AddString("RootCauseNotes", aggregate.RootCauseNotes, 4000)
            .AddDateTime("RootCauseRecordedAtUtc", aggregate.RootCauseRecordedAtUtc?.Value)
            .AddString("RegulatoryAgency", aggregate.RegulatoryAgency, 200)
            .AddString("RegulatoryReferenceNumber", aggregate.RegulatoryReferenceNumber, 100)
            .AddDateTime("RegulatoryFiledAtUtc", aggregate.RegulatoryFiledAtUtc?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_Accident_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[AccidentManagement].[usp_Accident_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Accident {aggregate.AccidentId.Value} was modified by another process or the ETag is stale.", Guid.Empty);
        }

        // Reconcile CorrectiveActions: CAPA items are never removed, only mutated -- upsert each.
        foreach (var ca in aggregate.CorrectiveActions)
        {
            var caParams = new StoredProcedureBuilder()
                .AddGuid("CorrectiveActionId", ca.CorrectiveActionId.Value)
                .AddGuid("AccidentId", aggregate.AccidentId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("Category", ca.Category.Code, 30)
                .AddString("Description", ca.Description, 2000)
                .AddGuid("ResponsiblePersonId", ca.ResponsiblePersonId.Value)
                .AddDateTime("DueAtUtc", ca.DueAtUtc.Value)
                .AddString("Status", ca.Status.ToString(), 30)
                .AddString("VerificationSteps", ca.VerificationSteps, 2000)
                .AddString("CompletionEvidence", ca.CompletionEvidence, 2000)
                .AddDateTime("CompletedAtUtc", ca.CompletedAtUtc?.Value)
                .AddDateTime("VerifiedAtUtc", ca.VerifiedAtUtc?.Value)
                .AddGuid("VerifiedBy", ca.VerifiedBy?.Value)
                .AddDateTime("CreatedAtUtc", ca.CreatedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[AccidentManagement].[usp_CorrectiveAction_Upsert]", caParams, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        await RelayDomainEventsAsync(aggregate, cancellationToken);
    }

    /// <summary>
    /// Enqueues the aggregate's pending domain events to both the intra-platform outbox and (for
    /// the curated subset with a registered cross-module DTO) Shared.EventBus, then flushes the
    /// cross-module relay now that the SQL transaction above has already committed
    /// (Publish-After-Commit; mirrors UnitOfWork.CommitAsync's discipline). This repository owns
    /// its own connection/transaction rather than going through TransactionBehavior/UnitOfWork, so
    /// it must drive the relay itself.
    /// </summary>
    private async Task RelayDomainEventsAsync(Accident aggregate, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
            crossModuleEventRelay?.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
            crossModuleEventRelayV2?.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();

        if (crossModuleEventRelay is { HasPendingEvents: true })
            await crossModuleEventRelay.FlushAsync(cancellationToken);

        if (crossModuleEventRelayV2 is { HasPendingEvents: true })
            await crossModuleEventRelayV2.FlushAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<CorrectiveActionRow>> GetCorrectiveActionRowsAsync(AccidentId id, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccidentId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        return await queryExecutor.QueryAsync<CorrectiveActionRow>(
            "[AccidentManagement].[usp_CorrectiveAction_GetByAccidentId]", parameters, cancellationToken);
    }

    /// <summary>Reconstructs an <see cref="Accident"/> aggregate from its flat read-model row and child rows.</summary>
    private static Accident Hydrate(AccidentRow row, IReadOnlyList<CorrectiveActionRow> caRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<Accident>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.AccidentId), AccidentId.FromGuid(row.AccidentId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.Status), Enum.Parse<AccidentStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.AccidentTitle), row.AccidentTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.Severity), AccidentSeverity.From(row.Severity));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.InjuredPersonCount), row.InjuredPersonCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.OccurredAtUtc), UtcTimestamp.From(row.OccurredAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.Category), InvestigationPhase.From(row.Category));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.SiteSecuredAndTriaged), row.SiteSecuredAndTriaged);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.InvestigatorId), row.InvestigatorId.HasValue ? new UserId(row.InvestigatorId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.InvestigatorAssignedAtUtc), row.InvestigatorAssignedAtUtc.HasValue ? UtcTimestamp.From(row.InvestigatorAssignedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RootCause), row.RootCause is null ? null : RootCauseCategory.From(row.RootCause));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RootCauseNotes), row.RootCauseNotes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RootCauseRecordedAtUtc), row.RootCauseRecordedAtUtc.HasValue ? UtcTimestamp.From(row.RootCauseRecordedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RegulatoryFilingRequired), row.RegulatoryFilingRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RegulatoryAgency), row.RegulatoryAgency);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RegulatoryReferenceNumber), row.RegulatoryReferenceNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.RegulatoryFiledAtUtc), row.RegulatoryFiledAtUtc.HasValue ? UtcTimestamp.From(row.RegulatoryFiledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.ReviewRequired), row.ReviewRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Accident.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAccidentDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_correctiveActions", new List<CorrectiveAction>());

        var caList = AggregateReflectionHydrator.GetPrivateCollectionField<Accident, List<CorrectiveAction>>(agg, "_correctiveActions");
        foreach (var c in caRows)
        {
            var child = AggregateReflectionHydrator.CreateUninitialized<CorrectiveAction>();
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.CorrectiveActionId), CorrectiveActionId.FromGuid(c.CorrectiveActionId));
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.AccidentId), AccidentId.FromGuid(c.AccidentId));
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.Category), CorrectiveActionCategory.From(c.Category));
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.Description), c.Description);
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.ResponsiblePersonId), new UserId(c.ResponsiblePersonId));
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.DueAtUtc), UtcTimestamp.From(c.DueAtUtc));
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.Status), Enum.Parse<CorrectiveActionStatus>(c.Status));
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.VerificationSteps), c.VerificationSteps);
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.CompletionEvidence), c.CompletionEvidence);
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.CompletedAtUtc), c.CompletedAtUtc.HasValue ? UtcTimestamp.From(c.CompletedAtUtc.Value) : (UtcTimestamp?)null);
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.VerifiedAtUtc), c.VerifiedAtUtc.HasValue ? UtcTimestamp.From(c.VerifiedAtUtc.Value) : (UtcTimestamp?)null);
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.VerifiedBy), c.VerifiedBy.HasValue ? new UserId(c.VerifiedBy.Value) : (UserId?)null);
            AggregateReflectionHydrator.SetProperty(child, nameof(CorrectiveAction.CreatedAtUtc), UtcTimestamp.From(c.CreatedAtUtc));

            caList.Add(child);
        }

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Accident_GetById</c>/<c>usp_Accident_GetAllActive</c>.</summary>
    private sealed record AccidentRow
    {
        public required Guid AccidentId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string AccidentTitle { get; init; }
        public required string Location { get; init; }
        public required string Severity { get; init; }
        public required int InjuredPersonCount { get; init; }
        public required DateTime OccurredAtUtc { get; init; }
        public required string Category { get; init; }
        public required bool SiteSecuredAndTriaged { get; init; }
        public Guid? InvestigatorId { get; init; }
        public DateTime? InvestigatorAssignedAtUtc { get; init; }
        public string? RootCause { get; init; }
        public string? RootCauseNotes { get; init; }
        public DateTime? RootCauseRecordedAtUtc { get; init; }
        public required bool RegulatoryFilingRequired { get; init; }
        public string? RegulatoryAgency { get; init; }
        public string? RegulatoryReferenceNumber { get; init; }
        public DateTime? RegulatoryFiledAtUtc { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public required Guid OwnerId { get; init; }
        public required bool ReviewRequired { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }

        /// <summary>
        /// Optimistic-concurrency token, projected from the row's native RowVersion column
        /// (<c>CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)</c>). Not yet consumed by
        /// any write path -- the <see cref="Accident"/> aggregate has no ETag concept and
        /// usp_Accident_Update does not take an @ETag parameter -- present so a future
        /// optimistic-concurrency pass has it available without another read-model change,
        /// matching the schema-only precedent set by Analytics.Culture's CultureReadModel.ETag.
        /// </summary>
        public string? ETag { get; init; }
    }

    /// <summary>Flat read-model row shape returned by <c>usp_CorrectiveAction_GetByAccidentId</c>.</summary>
    private sealed record CorrectiveActionRow
    {
        public required Guid CorrectiveActionId { get; init; }
        public required Guid AccidentId { get; init; }
        public required string Category { get; init; }
        public required string Description { get; init; }
        public required Guid ResponsiblePersonId { get; init; }
        public required DateTime DueAtUtc { get; init; }
        public required string Status { get; init; }
        public string? VerificationSteps { get; init; }
        public string? CompletionEvidence { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public DateTime? VerifiedAtUtc { get; init; }
        public Guid? VerifiedBy { get; init; }
        public required DateTime CreatedAtUtc { get; init; }

        /// <summary>See <see cref="AccidentRow.ETag"/> remarks -- not yet consumed by any write path.</summary>
        public string? ETag { get; init; }
    }
}
