// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Infrastructure
//  File:           LocalEmergencyCoordinationWriteDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer
//                  ILocalEmergencyCoordinationRepository (write-side aggregate repository). Named
//                  *WriteDapperRepository (not LocalEmergencyCoordinationDapperRepository, which
//                  backs the read-only Application-layer ILocalEmergencyCoordinationRepository) to
//                  avoid an unqualified-name collision between the two distinct
//                  ILocalEmergencyCoordinationRepository interfaces (Domain vs Application
//                  namespaces) -- mirrors EmergencyManagementWriteDapperRepository. Aggregate
//                  reconstruction uses AggregateReflectionHydrator (both for the aggregate root and
//                  for each private-constructor child entity) because the aggregate exposes only a
//                  private "for rehydration only" constructor. Child entity persistence uses
//                  idempotent INSERT-WHERE-NOT-EXISTS stored procedures re-submitted on every
//                  UpdateAsync from the aggregate's current in-memory collections (simpler than a
//                  table-valued-parameter diff, acceptable given the low write-volume nature of
//                  coordination workflows). Both AddAsync AND UpdateAsync enqueue domain events to
//                  the outbox after their write.
//                  Additive-only evolution.
// ============================================================================
using System.Data;
using Dapper;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Specifications;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;
using LocalEmergencyCoordinationEntities = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities;

namespace PlantSafety.Infrastructure.SafetyEvents.LocalEmergencyCoordination.Persistence;

/// <summary>Dapper-backed repository for the <see cref="LocalEmergencyCoordinationAggregate"/> aggregate root.</summary>
public sealed class LocalEmergencyCoordinationWriteDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<LocalEmergencyCoordinationWriteDapperRepository> logger)
    : ILocalEmergencyCoordinationRepository
{
    /// <inheritdoc/>
    public async Task<LocalEmergencyCoordinationAggregate?> GetByIdAsync(LocalEmergencyCoordinationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for LocalEmergencyCoordination {LocalEmergencyCoordinationId} Tenant {TenantId}", id.Value, tenantId);

        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LocalEmergencyCoordinationId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                "[SafetyEvents].[usp_LocalEmergencyCoordinationCases_GetById]",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var caseRow = await multi.ReadSingleOrDefaultAsync<LocalEmergencyCoordinationCaseRow>();
        if (caseRow is null) return null;

        var agreementRows = (await multi.ReadAsync<MutualAidAgreementRow>()).ToList();
        var planRows = (await multi.ReadAsync<JointResponsePlanRow>()).ToList();
        var liaisonRows = (await multi.ReadAsync<ExternalResponderLiaisonRow>()).ToList();
        var logRows = (await multi.ReadAsync<LiaisonLogEntryRow>()).ToList();
        var debriefRows = (await multi.ReadAsync<JointDebriefRecordRow>()).ToList();

        return Hydrate(caseRow, agreementRows, planRows, liaisonRows, logRows, debriefRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LocalEmergencyCoordinationAggregate>> ListAsync(LocalEmergencyCoordinationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List LocalEmergencyCoordination via specification {Specification}", specification.GetType().Name);

        var caseRows = await queryExecutor.QueryAsync<LocalEmergencyCoordinationCaseRow>(
            "[SafetyEvents].[usp_LocalEmergencyCoordinationCases_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<LocalEmergencyCoordinationAggregate>();

        foreach (var row in caseRows)
        {
            // Case-level fields only for the candidate scan; full child hydration happens via
            // GetByIdAsync once a caller has a concrete identity of interest.
            var aggregate = Hydrate(row, [], [], [], [], []);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(LocalEmergencyCoordinationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add LocalEmergencyCoordination {LocalEmergencyCoordinationId}", aggregate.LocalEmergencyCoordinationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("IncidentId", aggregate.IncidentId)
            .AddString("CaseName", aggregate.CaseName, 200)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_LocalEmergencyCoordinationCases_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(LocalEmergencyCoordinationAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update LocalEmergencyCoordination {LocalEmergencyCoordinationId}", aggregate.LocalEmergencyCoordinationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var caseParameters = new StoredProcedureBuilder()
            .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_LocalEmergencyCoordinationCases_Update]", caseParameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"LocalEmergencyCoordination {aggregate.LocalEmergencyCoordinationId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        foreach (var agreement in aggregate.MutualAidAgreements)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("MutualAidAgreementId", agreement.MutualAidAgreementId.Value)
                .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("AgencyName", agreement.AgencyName, 200)
                .AddString("Discipline", agreement.Discipline, 50)
                .AddInt("DocumentVersion", agreement.DocumentVersion)
                .AddDateTime("EffectiveFromUtc", agreement.EffectiveFromUtc.Value)
                .AddDateTime("RenewalDueAtUtc", agreement.RenewalDueAtUtc.Value)
                .AddString("RenewalStatus", agreement.RenewalStatus.ToString(), 30)
                .AddDateTime("EstablishedAtUtc", agreement.EstablishedAtUtc.Value)
                .AddGuid("EstablishedBy", agreement.EstablishedBy.Value)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_MutualAidAgreements_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var plan in aggregate.JointResponsePlans)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("JointResponsePlanId", plan.JointResponsePlanId.Value)
                .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("IncidentId", plan.IncidentId)
                .AddString("PlanName", plan.PlanName, 200)
                .AddBool("IsActive", plan.IsActive)
                .AddDateTime("ActivatedAtUtc", plan.ActivatedAtUtc.Value)
                .AddGuid("ActivatedBy", plan.ActivatedBy.Value)
                .AddDateTime("DeactivatedAtUtc", plan.DeactivatedAtUtc?.Value)
                .AddGuid("DeactivatedBy", plan.DeactivatedBy?.Value)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_JointResponsePlans_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var liaison in aggregate.ExternalResponderLiaisons)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("ExternalResponderLiaisonId", liaison.ExternalResponderLiaisonId.Value)
                .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("IncidentId", liaison.IncidentId)
                .AddString("AgencyName", liaison.AgencyName, 200)
                .AddGuid("LiaisonUserId", liaison.LiaisonUserId.Value)
                .AddDateTime("AssignedAtUtc", liaison.AssignedAtUtc.Value)
                .AddGuid("AssignedBy", liaison.AssignedBy.Value)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_ExternalResponderLiaisons_Insert]", p, tx, cancellationToken: cancellationToken);

            foreach (var entry in liaison.LogEntries)
            {
                var lp = new StoredProcedureBuilder()
                    .AddGuid("LiaisonLogEntryId", entry.LiaisonLogEntryId.Value)
                    .AddGuid("ExternalResponderLiaisonId", liaison.ExternalResponderLiaisonId.Value)
                    .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
                    .AddGuid("TenantId", aggregate.TenantId)
                    .AddGuid("IncidentId", liaison.IncidentId)
                    .AddString("AgencyName", liaison.AgencyName, 200)
                    .AddString("Message", entry.Message, 2000)
                    .AddDateTime("LoggedAtUtc", entry.LoggedAtUtc.Value)
                    .AddGuid("LoggedBy", entry.LoggedBy.Value)
                    .Build();
                await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_LiaisonLogEntries_Insert]", lp, tx, cancellationToken: cancellationToken);
            }
        }

        foreach (var debrief in aggregate.JointDebriefRecords)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("JointDebriefRecordId", debrief.JointDebriefRecordId.Value)
                .AddGuid("LocalEmergencyCoordinationId", aggregate.LocalEmergencyCoordinationId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("IncidentId", debrief.IncidentId)
                .AddString("AgenciesInvolved", debrief.AgenciesInvolved, 500)
                .AddString("Summary", debrief.Summary, 4000)
                .AddDateTime("RecordedAtUtc", debrief.RecordedAtUtc.Value)
                .AddGuid("RecordedBy", debrief.RecordedBy.Value)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_JointDebriefRecords_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="LocalEmergencyCoordinationAggregate"/> aggregate from its flat read-model rows.</summary>
    private static LocalEmergencyCoordinationAggregate Hydrate(
        LocalEmergencyCoordinationCaseRow row,
        List<MutualAidAgreementRow> agreementRows,
        List<JointResponsePlanRow> planRows,
        List<ExternalResponderLiaisonRow> liaisonRows,
        List<LiaisonLogEntryRow> logRows,
        List<JointDebriefRecordRow> debriefRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<LocalEmergencyCoordinationAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.LocalEmergencyCoordinationId), LocalEmergencyCoordinationId.FromGuid(row.LocalEmergencyCoordinationId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.Status), Enum.Parse<LocalEmergencyCoordinationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.IncidentId), row.IncidentId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.CaseName), row.CaseName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(LocalEmergencyCoordinationAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ILocalEmergencyCoordinationDomainEvent>());

        var agreements = agreementRows.Select(HydrateAgreement).ToList();
        var plans = planRows.Select(HydratePlan).ToList();
        var liaisons = liaisonRows.Select(l => HydrateLiaison(l, logRows.Where(x => x.ExternalResponderLiaisonId == l.ExternalResponderLiaisonId).ToList())).ToList();
        var debriefs = debriefRows.Select(HydrateDebrief).ToList();

        AggregateReflectionHydrator.SetField(agg, "_mutualAidAgreements", agreements);
        AggregateReflectionHydrator.SetField(agg, "_jointResponsePlans", plans);
        AggregateReflectionHydrator.SetField(agg, "_externalResponderLiaisons", liaisons);
        AggregateReflectionHydrator.SetField(agg, "_jointDebriefRecords", debriefs);

        return agg;
    }

    private static LocalEmergencyCoordinationEntities.MutualAidAgreement HydrateAgreement(MutualAidAgreementRow row)
    {
        var entity = AggregateReflectionHydrator.CreateUninitialized<LocalEmergencyCoordinationEntities.MutualAidAgreement>();
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.MutualAidAgreementId), MutualAidAgreementId.FromGuid(row.MutualAidAgreementId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.LocalEmergencyCoordinationId), LocalEmergencyCoordinationId.FromGuid(row.LocalEmergencyCoordinationId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.AgencyName), row.AgencyName);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.Discipline), row.Discipline);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.DocumentVersion), row.DocumentVersion);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.EffectiveFromUtc), UtcTimestamp.From(row.EffectiveFromUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.RenewalDueAtUtc), UtcTimestamp.From(row.RenewalDueAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.RenewalStatus), Enum.Parse<MutualAidAgreementRenewalStatus>(row.RenewalStatus));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.EstablishedAtUtc), UtcTimestamp.From(row.EstablishedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.MutualAidAgreement.EstablishedBy), new UserId(row.EstablishedBy));
        return entity;
    }

    private static LocalEmergencyCoordinationEntities.JointResponsePlan HydratePlan(JointResponsePlanRow row)
    {
        var entity = AggregateReflectionHydrator.CreateUninitialized<LocalEmergencyCoordinationEntities.JointResponsePlan>();
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.JointResponsePlanId), JointResponsePlanId.FromGuid(row.JointResponsePlanId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.LocalEmergencyCoordinationId), LocalEmergencyCoordinationId.FromGuid(row.LocalEmergencyCoordinationId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.IncidentId), row.IncidentId);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.PlanName), row.PlanName);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.IsActive), row.IsActive);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.ActivatedAtUtc), UtcTimestamp.From(row.ActivatedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.ActivatedBy), new UserId(row.ActivatedBy));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.DeactivatedAtUtc), row.DeactivatedAtUtc.HasValue ? UtcTimestamp.From(row.DeactivatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointResponsePlan.DeactivatedBy), row.DeactivatedBy.HasValue ? new UserId(row.DeactivatedBy.Value) : (UserId?)null);
        return entity;
    }

    private static LocalEmergencyCoordinationEntities.ExternalResponderLiaison HydrateLiaison(ExternalResponderLiaisonRow row, List<LiaisonLogEntryRow> logRows)
    {
        var entity = AggregateReflectionHydrator.CreateUninitialized<LocalEmergencyCoordinationEntities.ExternalResponderLiaison>();
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.ExternalResponderLiaisonId), ExternalResponderLiaisonId.FromGuid(row.ExternalResponderLiaisonId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.LocalEmergencyCoordinationId), LocalEmergencyCoordinationId.FromGuid(row.LocalEmergencyCoordinationId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.IncidentId), row.IncidentId);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.AgencyName), row.AgencyName);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.LiaisonUserId), new UserId(row.LiaisonUserId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.AssignedAtUtc), UtcTimestamp.From(row.AssignedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.ExternalResponderLiaison.AssignedBy), new UserId(row.AssignedBy));
        AggregateReflectionHydrator.SetField(entity, "_logEntries", logRows.Select(HydrateLogEntry).ToList());

        return entity;
    }

    private static LocalEmergencyCoordinationEntities.LiaisonLogEntry HydrateLogEntry(LiaisonLogEntryRow row)
    {
        var entity = AggregateReflectionHydrator.CreateUninitialized<LocalEmergencyCoordinationEntities.LiaisonLogEntry>();
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.LiaisonLogEntry.LiaisonLogEntryId), LiaisonLogEntryId.FromGuid(row.LiaisonLogEntryId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.LiaisonLogEntry.ExternalResponderLiaisonId), ExternalResponderLiaisonId.FromGuid(row.ExternalResponderLiaisonId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.LiaisonLogEntry.Message), row.Message);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.LiaisonLogEntry.LoggedAtUtc), UtcTimestamp.From(row.LoggedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.LiaisonLogEntry.LoggedBy), new UserId(row.LoggedBy));
        return entity;
    }

    private static LocalEmergencyCoordinationEntities.JointDebriefRecord HydrateDebrief(JointDebriefRecordRow row)
    {
        var entity = AggregateReflectionHydrator.CreateUninitialized<LocalEmergencyCoordinationEntities.JointDebriefRecord>();
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.JointDebriefRecordId), JointDebriefRecordId.FromGuid(row.JointDebriefRecordId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.LocalEmergencyCoordinationId), LocalEmergencyCoordinationId.FromGuid(row.LocalEmergencyCoordinationId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.IncidentId), row.IncidentId);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.AgenciesInvolved), row.AgenciesInvolved);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.Summary), row.Summary);
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.RecordedAtUtc), UtcTimestamp.From(row.RecordedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(LocalEmergencyCoordinationEntities.JointDebriefRecord.RecordedBy), new UserId(row.RecordedBy));
        return entity;
    }

    private sealed record LocalEmergencyCoordinationCaseRow
    {
        public string? ETag { get; init; }
        public required Guid LocalEmergencyCoordinationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public Guid? IncidentId { get; init; }
        public required string CaseName { get; init; }
        public required string Status { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
    }

    private sealed record MutualAidAgreementRow
    {
        public required Guid MutualAidAgreementId { get; init; }
        public required Guid LocalEmergencyCoordinationId { get; init; }
        public required string AgencyName { get; init; }
        public required string Discipline { get; init; }
        public int DocumentVersion { get; init; }
        public required DateTime EffectiveFromUtc { get; init; }
        public required DateTime RenewalDueAtUtc { get; init; }
        public required string RenewalStatus { get; init; }
        public required DateTime EstablishedAtUtc { get; init; }
        public required Guid EstablishedBy { get; init; }
    }

    private sealed record JointResponsePlanRow
    {
        public required Guid JointResponsePlanId { get; init; }
        public required Guid LocalEmergencyCoordinationId { get; init; }
        public Guid? IncidentId { get; init; }
        public required string PlanName { get; init; }
        public bool IsActive { get; init; }
        public required DateTime ActivatedAtUtc { get; init; }
        public required Guid ActivatedBy { get; init; }
        public DateTime? DeactivatedAtUtc { get; init; }
        public Guid? DeactivatedBy { get; init; }
    }

    private sealed record ExternalResponderLiaisonRow
    {
        public required Guid ExternalResponderLiaisonId { get; init; }
        public required Guid LocalEmergencyCoordinationId { get; init; }
        public Guid? IncidentId { get; init; }
        public required string AgencyName { get; init; }
        public required Guid LiaisonUserId { get; init; }
        public required DateTime AssignedAtUtc { get; init; }
        public required Guid AssignedBy { get; init; }
    }

    private sealed record LiaisonLogEntryRow
    {
        public required Guid LiaisonLogEntryId { get; init; }
        public required Guid ExternalResponderLiaisonId { get; init; }
        public Guid? IncidentId { get; init; }
        public required string Message { get; init; }
        public required DateTime LoggedAtUtc { get; init; }
        public required Guid LoggedBy { get; init; }
    }

    private sealed record JointDebriefRecordRow
    {
        public required Guid JointDebriefRecordId { get; init; }
        public required Guid LocalEmergencyCoordinationId { get; init; }
        public Guid? IncidentId { get; init; }
        public required string AgenciesInvolved { get; init; }
        public required string Summary { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }
}
