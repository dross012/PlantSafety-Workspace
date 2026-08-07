// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Infrastructure
//  File:           IncidentReportDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IIncidentReportRepository (Domain repository
//                  interface for the IncidentReport aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because IncidentReport exposes only a private "for
//                  rehydration only" constructor and private-setter properties. ListAsync pushes
//                  the optional incident-date window down to usp_PHMSAIncidentReport_GetAllActive
//                  and then filters by tenant in-memory over the returned candidate set, since
//                  that proc carries no @TenantId parameter (mirrors PermitsDapperRepository.
//                  ListAsync's identical in-memory-filtering precedent). Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write -- a
//                  platform-wide bug fixed elsewhere in this program was exactly UpdateAsync
//                  forgetting to do this, so it is deliberately present here from day one.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag from day one. Mirrors PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Events;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Incidents.Persistence;

/// <summary>Dapper-backed repository for the <see cref="IncidentReport"/> aggregate root.</summary>
public sealed class IncidentReportDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<IncidentReportDapperRepository> logger)
    : IIncidentReportRepository
{
    /// <inheritdoc/>
    public async Task<IncidentReport?> GetByIdAsync(IncidentReportId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for IncidentReport {IncidentReportId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentReportId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<IncidentReportRow>(
            "[Compliance].[usp_PHMSAIncidentReport_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentReport>> ListAsync(Guid tenantId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List IncidentReport for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .Build();

        var rows = await queryExecutor.QueryAsync<IncidentReportRow>(
            "[Compliance].[usp_PHMSAIncidentReport_GetAllActive]", parameters, cancellationToken);

        return rows
            .Where(row => row.TenantId == tenantId)
            .Select(Hydrate)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(IncidentReport aggregate, Guid tenantId, Guid? siteId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add IncidentReport {IncidentReportId} Tenant {TenantId}", aggregate.Id.Value, tenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentReportId", aggregate.Id.Value)
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("IncidentType", aggregate.IncidentType.ToString(), 30)
            .AddGuid("MaterialInvolvedId", aggregate.MaterialInvolvedId?.Value)
            .AddString("MaterialInvolvedDescription", aggregate.MaterialInvolvedDescription, size: -1)
            .AddInt("Injuries", aggregate.Injuries)
            .AddBool("FatalitiesInvolved", aggregate.FatalitiesInvolved)
            .AddDecimal("PropertyDamageEstimateUsd", aggregate.PropertyDamageEstimateUsd)
            .AddString("Narrative", aggregate.Narrative, size: -1)
            .AddDateTime("IncidentDateUtc", aggregate.IncidentDateUtc)
            .AddString("Location", aggregate.Location, 300)
            .AddBool("IsReportableToPhmsa", aggregate.IsReportableToPhmsa)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_PHMSAIncidentReport_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised by the
        // aggregate during this write so TransactionalUnitOfWork.CommitAsync (invoked by
        // TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, tenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(IncidentReport aggregate, Guid tenantId, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update IncidentReport {IncidentReportId} Tenant {TenantId}", aggregate.Id.Value, tenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentReportId", aggregate.Id.Value)
            .AddGuid("TenantId", tenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("IncidentType", aggregate.IncidentType.ToString(), 30)
            .AddGuid("MaterialInvolvedId", aggregate.MaterialInvolvedId?.Value)
            .AddString("MaterialInvolvedDescription", aggregate.MaterialInvolvedDescription, size: -1)
            .AddInt("Injuries", aggregate.Injuries)
            .AddBool("FatalitiesInvolved", aggregate.FatalitiesInvolved)
            .AddDecimal("PropertyDamageEstimateUsd", aggregate.PropertyDamageEstimateUsd)
            .AddString("Narrative", aggregate.Narrative, size: -1)
            .AddDateTime("IncidentDateUtc", aggregate.IncidentDateUtc)
            .AddString("Location", aggregate.Location, 300)
            .AddBool("IsReportableToPhmsa", aggregate.IsReportableToPhmsa)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PHMSAIncidentReport_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_PHMSAIncidentReport_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"IncidentReport {aggregate.Id.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition (Submit, BeginReview, Close) would silently
        // drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, tenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="IncidentReport"/> aggregate from its flat read-model row.</summary>
    private static IncidentReport Hydrate(IncidentReportRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<IncidentReport>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.Id), IncidentReportId.FromGuid(row.IncidentReportId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.Status), Enum.Parse<IncidentReportStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.IncidentType), Enum.Parse<IncidentType>(row.IncidentType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.MaterialInvolvedId), row.MaterialInvolvedId.HasValue ? HazmatMaterialId.FromGuid(row.MaterialInvolvedId.Value) : (HazmatMaterialId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.MaterialInvolvedDescription), row.MaterialInvolvedDescription);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.Injuries), row.Injuries);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.FatalitiesInvolved), row.FatalitiesInvolved);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.PropertyDamageEstimateUsd), row.PropertyDamageEstimateUsd);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.Narrative), row.Narrative);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.IncidentDateUtc), row.IncidentDateUtc);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.IsReportableToPhmsa), row.IsReportableToPhmsa);
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(IncidentReport.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IIncidentReportDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PHMSAIncidentReport_GetById</c>/<c>usp_PHMSAIncidentReport_GetAllActive</c>.</summary>
    private sealed record IncidentReportRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PHMSAIncidentReport_GetById</c>/<c>usp_PHMSAIncidentReport_GetAllActive</c>.
        /// Not consumed by aggregate hydration -- callers obtain the ETag for a subsequent write
        /// from a prior single-record read (see the Application-layer IncidentReportDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid IncidentReportId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string IncidentType { get; init; }
        public Guid? MaterialInvolvedId { get; init; }
        public string? MaterialInvolvedDescription { get; init; }
        public required int Injuries { get; init; }
        public required bool FatalitiesInvolved { get; init; }
        public decimal? PropertyDamageEstimateUsd { get; init; }
        public required string Narrative { get; init; }
        public required DateTime IncidentDateUtc { get; init; }
        public required string Location { get; init; }
        public required bool IsReportableToPhmsa { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
