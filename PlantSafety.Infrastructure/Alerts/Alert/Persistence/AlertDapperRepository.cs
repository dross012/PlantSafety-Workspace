// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Infrastructure
//  File:           AlertDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:
//      Dapper write-side repository for the Alert aggregate. Implements both the Domain
//      repository contract (PlantSafety.Domain.Alerts.Alert.Interfaces.IAlertRepository) and the
//      Application command-repository contract
//      (PlantSafety.Application.Alerts.Commands.IAlertCommandRepository) -- their method shapes
//      are identical (GetByIdAsync/AddAsync/UpdateAsync), so a single class satisfies both
//      without duplicating persistence logic. All DB access is via stored procedures under the
//      [Alerts] schema (distinct from, and never touching, the pre-existing
//      [OperationalSafety].[Alerts] table/procedures owned by the OperationalSafety.Alerts
//      "canonical reference module").
//
//      REMARK -- Domain interface tenant-scoping gap (flagged, not silently worked around):
//      IAlertRepository.GetByIdAsync/ListAsync carry no TenantId parameter, but every
//      tenant-scoped table in this platform is protected by a strict, deny-by-default
//      Row-Level-Security policy keyed on SESSION_CONTEXT('TenantId') (see
//      90-row-level-security.sql). Per the governance instruction to apply the stricter
//      compliance rule and never loosen a control to make a check pass, these two methods are
//      implemented WITHOUT setting a tenant session context (no tenant id is available to set),
//      so under the platform's default security policy they will return no rows unless invoked
//      on a connection/transaction where SESSION_CONTEXT('TenantId') has already been
//      established for the correct tenant by an outer Application-layer call in the same unit
//      of work. This is a pre-existing Domain-interface design gap, not introduced here.
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Alerts.Commands;
using PlantSafety.Domain.Alerts.Alert.Entities;
using PlantSafety.Domain.Alerts.Alert.Enums;
using PlantSafety.Domain.Alerts.Alert.Specifications;
using PlantSafety.Domain.Alerts.Alert.ValueObjects;
using PlantSafety.Infrastructure.Common.Persistence;
using DomainAlert = PlantSafety.Domain.Alerts.Alert.Aggregates.Alert;
using DomainIAlertRepository = PlantSafety.Domain.Alerts.Alert.Interfaces.IAlertRepository;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Infrastructure.Alerts.Alert.Persistence;

/// <summary>
/// Dapper-backed write-side repository for the <see cref="DomainAlert"/> aggregate. Executes
/// stored procedures under the <c>[Alerts]</c> schema for all persistence operations.
/// </summary>
public sealed class AlertDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AlertDapperRepository> logger)
    : DomainIAlertRepository, IAlertCommandRepository
{
    /// <inheritdoc cref="DomainIAlertRepository.GetByIdAsync"/>
    public async Task<DomainAlert?> GetByIdAsync(AlertId alertId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Alert {AlertId}", alertId.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AlertId", alertId.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AlertRow>("[Alerts].[usp_Alert_GetById]", parameters, cancellationToken);
        if (row is null) return null;

        var deliveryRows = await queryExecutor.QueryAsync<AlertDeliveryRecordRow>(
            "[Alerts].[usp_Alert_GetDeliveryRecords]", new StoredProcedureBuilder().AddGuid("AlertId", alertId.Value).Build(), cancellationToken);
        var responseRows = await queryExecutor.QueryAsync<AlertResponseActionRow>(
            "[Alerts].[usp_Alert_GetResponseActions]", new StoredProcedureBuilder().AddGuid("AlertId", alertId.Value).Build(), cancellationToken);

        return Rehydrate(row, deliveryRows, responseRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DomainAlert>> ListAsync(AlertSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for Alert (in-memory specification filter)");

        var rows = await queryExecutor.QueryAsync<AlertRow>("[Alerts].[usp_Alert_ListAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        var result = new List<DomainAlert>();

        foreach (var row in rows)
        {
            var deliveryRows = await queryExecutor.QueryAsync<AlertDeliveryRecordRow>(
                "[Alerts].[usp_Alert_GetDeliveryRecords]", new StoredProcedureBuilder().AddGuid("AlertId", row.AlertId).Build(), cancellationToken);
            var responseRows = await queryExecutor.QueryAsync<AlertResponseActionRow>(
                "[Alerts].[usp_Alert_GetResponseActions]", new StoredProcedureBuilder().AddGuid("AlertId", row.AlertId).Build(), cancellationToken);

            var aggregate = Rehydrate(row, deliveryRows, responseRows);
            if (predicate(aggregate))
                result.Add(aggregate);
        }

        return result;
    }

    /// <inheritdoc cref="DomainIAlertRepository.AddAsync"/>
    public async Task AddAsync(DomainAlert aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Insert Alert {AlertId} for tenant {TenantId}", aggregate.AlertId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var correlationId = Guid.NewGuid();
        var actorId = aggregate.CreatedBy?.Value ?? Guid.Empty;

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AlertId", aggregate.AlertId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("CategoryCode", aggregate.Category.Code, 50)
            .AddString("SeverityCode", aggregate.Severity.Code, 20)
            .AddString("SourceType", aggregate.SourceType.ToString(), 30)
            .AddString("Title", aggregate.Title, 200)
            .AddString("LocationZoneName", aggregate.Location.ZoneName, 200)
            .AddDecimal("LocationLatitude", (decimal?)aggregate.Location.Latitude)
            .AddDecimal("LocationLongitude", (decimal?)aggregate.Location.Longitude)
            .AddString("LocationIndoorPositionReference", aggregate.Location.IndoorPositionReference, 200)
            .AddDateTime("TriggeredAtUtc", aggregate.TriggeredAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy?.Value)
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, DateTime.UtcNow)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Alerts].[usp_Alert_Trigger]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc cref="DomainIAlertRepository.UpdateAsync"/>
    public async Task UpdateAsync(DomainAlert aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Alert {AlertId} for tenant {TenantId}", aggregate.AlertId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var correlationId = Guid.NewGuid();
        var actorId = Guid.Empty;
        var nowUtc = DateTime.UtcNow;

        // Sync additive child collections first (delivery records / response actions are
        // append-only on the aggregate; insert any not yet persisted).
        var persistedDeliveryIds = (await queryExecutor.QueryAsync<AlertDeliveryRecordRow>(
                "[Alerts].[usp_Alert_GetDeliveryRecords]", new StoredProcedureBuilder().AddGuid("AlertId", aggregate.AlertId.Value).Build(), cancellationToken))
            .Select(r => r.DeliveryRecordId).ToHashSet();

        foreach (var delivery in aggregate.DeliveryRecords.Where(d => !persistedDeliveryIds.Contains(d.DeliveryRecordId)))
        {
            var deliveryParameters = new StoredProcedureBuilder()
                .AddGuid("DeliveryRecordId", delivery.DeliveryRecordId)
                .AddGuid("AlertId", aggregate.AlertId.Value)
                .AddString("Channel", delivery.Channel.ToString(), 30)
                .AddBool("Succeeded", delivery.Succeeded)
                .AddDateTime("AttemptedAtUtc", delivery.AttemptedAtUtc.Value)
                .AddString("FailureReason", delivery.FailureReason, 500)
                .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Alerts].[usp_Alert_RecordDelivery]", deliveryParameters, tx, cancellationToken: cancellationToken);
        }

        var persistedResponseIds = (await queryExecutor.QueryAsync<AlertResponseActionRow>(
                "[Alerts].[usp_Alert_GetResponseActions]", new StoredProcedureBuilder().AddGuid("AlertId", aggregate.AlertId.Value).Build(), cancellationToken))
            .Select(r => r.ResponseActionId).ToHashSet();

        foreach (var response in aggregate.ResponseActions.Where(r => !persistedResponseIds.Contains(r.ResponseActionId)))
        {
            var responseParameters = new StoredProcedureBuilder()
                .AddGuid("ResponseActionId", response.ResponseActionId)
                .AddGuid("AlertId", aggregate.AlertId.Value)
                .AddString("ActionDescription", response.ActionDescription, 500)
                .AddDateTime("TakenAtUtc", response.TakenAtUtc.Value)
                .AddString("TakenByActorKind", response.TakenBy.Kind.ToString(), 20)
                .AddGuid("TakenByUserId", response.TakenBy.UserId?.Value)
                .AddString("Notes", response.Notes, 2000)
                .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Alerts].[usp_Alert_RecordResponseAction]", responseParameters, tx, cancellationToken: cancellationToken);
        }

        var stateParameters = new StoredProcedureBuilder()
            .AddGuid("AlertId", aggregate.AlertId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("SeverityCode", aggregate.Severity.Code, 20)
            .AddDateTime("AcknowledgedAtUtc", aggregate.AcknowledgedAtUtc?.Value)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddString("CloseOutVerification", aggregate.CloseOutVerification, 4000)
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, nowUtc)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Alerts].[usp_Alert_UpdateState]", stateParameters, tx, cancellationToken: cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    // ---- Rehydration ---------------------------------------------------------

    private static DomainAlert Rehydrate(AlertRow row, IReadOnlyList<AlertDeliveryRecordRow> deliveryRows, IReadOnlyList<AlertResponseActionRow> responseRows)
    {
        var alert = AggregateReflectionRehydrator.CreateUninitialized<DomainAlert>();

        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.AlertId), new AlertId(row.AlertId));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.TenantId), row.TenantId);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.SiteId), row.SiteId);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.Status), Enum.Parse<AlertStatus>(row.Status));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.Category), AlertCategory.From(row.CategoryCode));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.Severity), AlertSeverity.From(row.SeverityCode));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.SourceType), Enum.Parse<AlertSourceType>(row.SourceType));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.Title), row.Title);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.Location),
            AlertLocation.Create(row.LocationZoneName, row.LocationLatitude, row.LocationLongitude, row.LocationIndoorPositionReference));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.TriggeredAtUtc), UtcTimestamp.From(row.TriggeredAtUtc));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.AcknowledgedAtUtc), row.AcknowledgedAtUtc.HasValue ? UtcTimestamp.From(row.AcknowledgedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.ResolvedAtUtc), row.ResolvedAtUtc.HasValue ? UtcTimestamp.From(row.ResolvedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.CloseOutVerification), row.CloseOutVerification);
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionRehydrator.SetProperty(alert, nameof(DomainAlert.CreatedBy), row.CreatedBy.HasValue ? UserId.FromGuid(row.CreatedBy.Value) : (UserId?)null);

        // Rehydration bug fix (SmokeHarness Check 2): RuntimeHelpers.GetUninitializedObject
        // bypasses ALL field initializers, so "_deliveryRecords"/"_responseActions" start out
        // null, not the empty list their inline initializers declare. GetListField only reads
        // the current field value (it does not create one), so any Add() below -- or any public
        // getter such as Alert.DeliveryRecords -- would NullReferenceException without this.
        AggregateReflectionRehydrator.SetField(alert, "_deliveryRecords", new List<AlertDeliveryRecord>());
        AggregateReflectionRehydrator.SetField(alert, "_responseActions", new List<AlertResponseActionEntry>());

        var deliveryList = AggregateReflectionRehydrator.GetListField<DomainAlert, AlertDeliveryRecord>(alert, "_deliveryRecords");
        foreach (var d in deliveryRows)
        {
            deliveryList.Add(new AlertDeliveryRecord(
                d.DeliveryRecordId, new AlertId(d.AlertId), Enum.Parse<AlertDeliveryChannel>(d.Channel),
                d.Succeeded, UtcTimestamp.From(d.AttemptedAtUtc), d.FailureReason));
        }

        var responseList = AggregateReflectionRehydrator.GetListField<DomainAlert, AlertResponseActionEntry>(alert, "_responseActions");
        foreach (var r in responseRows)
        {
            responseList.Add(new AlertResponseActionEntry(
                r.ResponseActionId, new AlertId(r.AlertId), r.ActionDescription, UtcTimestamp.From(r.TakenAtUtc),
                ToActor(r.TakenByActorKind, r.TakenByUserId), r.Notes));
        }

        // Rehydration bug fix (SmokeHarness Check 2): _domainEvents was never initialized here,
        // so any lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate
        // (e.g. Acknowledge()) threw NullReferenceException. RuntimeHelpers.GetUninitializedObject
        // leaves this readonly field null since it bypasses field initializers entirely.
        AggregateReflectionRehydrator.SetField(alert, "_domainEvents", new List<PlantSafety.Domain.Alerts.Alert.Events.IAlertDomainEvent>());

        return alert;
    }

    private static PerformedByActor ToActor(string actorKindCode, Guid? userId)
    {
        var kind = Enum.Parse<ActorKind>(actorKindCode);
        return kind switch
        {
            ActorKind.User when userId.HasValue => PerformedByActor.User(UserId.FromGuid(userId.Value)),
            ActorKind.AiAgent => PerformedByActor.AiAgent(),
            ActorKind.External => PerformedByActor.External(),
            _ => PerformedByActor.System(),
        };
    }

    // ---- Dapper row-mapping POCOs ---------------------------------------------

    private sealed class AlertRow
    {
        public Guid AlertId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? SiteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string SeverityCode { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LocationZoneName { get; set; } = string.Empty;
        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }
        public string? LocationIndoorPositionReference { get; set; }
        public DateTime TriggeredAtUtc { get; set; }
        public DateTime? AcknowledgedAtUtc { get; set; }
        public DateTime? ResolvedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public string? CloseOutVerification { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid? CreatedBy { get; set; }
    }

    private sealed class AlertDeliveryRecordRow
    {
        public Guid DeliveryRecordId { get; set; }
        public Guid AlertId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public bool Succeeded { get; set; }
        public DateTime AttemptedAtUtc { get; set; }
        public string? FailureReason { get; set; }
    }

    private sealed class AlertResponseActionRow
    {
        public Guid ResponseActionId { get; set; }
        public Guid AlertId { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public DateTime TakenAtUtc { get; set; }
        public string TakenByActorKind { get; set; } = string.Empty;
        public Guid? TakenByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
