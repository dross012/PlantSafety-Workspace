// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivity.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Intelligence.DataLake bounded context, backing
//                  [Intelligence].[DataLakeActivity] (usp_DataLakeActivity_Insert /
//                  usp_DataLakeActivity_Update / usp_DataLakeActivity_GetById via the Domain-layer
//                  IDataLakeActivityRepository / DataLakeActivityAggregateDapperRepository). Before
//                  this pass, this module contained only a "DataLakeStub.cs" placeholder marker
//                  class ("Replace with the full aggregate structure") -- there was no domain model
//                  at all backing this feature; only a read-only date-range list query
//                  (PlantSafety.Application.Intelligence.DataLake.Queries.GetDataLakeQuery via
//                  PlantSafety.Application.Intelligence.DataLake.Repositories.IDataLakeRepository /
//                  PlantSafety.Infrastructure.Intelligence.DataLake.Persistence
//                  .DataLakeDapperRepository calling usp_DataLake_GetDataLakeRecords) existed, and
//                  it is left entirely untouched by this pass -- it is a different, coarser
//                  reporting query, not the by-id/lifecycle path built here. Field shape
//                  (DataLakeRecordId, TenantId, SiteId, DatasetName, Zone, RecordCount, Status,
//                  ProcessedAtUtc, ProcessedBy) is grounded in that pre-existing read model's
//                  actual DTO (PlantSafety.Application.Intelligence.DataLake.Models
//                  .DataLakeItemDto), not invented from scratch. Lifecycle states
//                  (Ingested/Processing/Curated/Failed) are taken from DataLakeItemDto.Status's
//                  documented vocabulary plus the platform-standard terminal Archived state.
//                  Mirrors PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA's
//                  guarded-transition/domain-event/exception conventions (EnsureStatusIsOneOf +
//                  DataLakeActivityLifecycleRules.CanX guard pattern).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.DataLake.Enums;
using PlantSafety.Domain.Intelligence.DataLake.Events;
using PlantSafety.Domain.Intelligence.DataLake.Exceptions;
using PlantSafety.Domain.Intelligence.DataLake.Rules;
using PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.DataLake.Aggregates;

/// <summary>Aggregate root for the Intelligence.DataLake bounded context.</summary>
public sealed class DataLakeActivity
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this DataLakeActivity aggregate.</summary>
    public DataLakeActivityId DataLakeActivityId { get; private set; }

    /// <summary>Tenant that owns this data lake activity record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DataLakeActivityStatus Status { get; private set; }

    // ---- DataLakeActivity-specific business properties ----------------------

    /// <summary>Name of the dataset associated with this record.</summary>
    public string DatasetName { get; private set; } = string.Empty;

    /// <summary>Data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</summary>
    public string Zone { get; private set; } = string.Empty;

    /// <summary>Number of records processed in this activity.</summary>
    public long? RecordCount { get; private set; }

    /// <summary>UTC timestamp when the activity was processed (or first ingested).</summary>
    public UtcTimestamp ProcessedAtUtc { get; private set; }

    /// <summary>Identity of the user or process that performed the activity.</summary>
    public UserId ProcessedBy { get; private set; }

    // ---- Lifecycle attributes (StartProcessing/Curate/Fail/Archive) ---------

    /// <summary>Identity of the user who started processing (transitioned to Processing).</summary>
    public UserId? ProcessingStartedBy { get; private set; }

    /// <summary>UTC instant the activity transitioned to Processing.</summary>
    public UtcTimestamp? ProcessingStartedAtUtc { get; private set; }

    /// <summary>UTC instant the activity's data was curated.</summary>
    public UtcTimestamp? CuratedAtUtc { get; private set; }

    /// <summary>Failure reason recorded when the activity failed, if any.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>UTC instant the activity failed.</summary>
    public UtcTimestamp? FailedAtUtc { get; private set; }

    /// <summary>UTC instant the activity was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IDataLakeActivityDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDataLakeActivityDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // For rehydration only.
    private DataLakeActivity() { }

    // ---- Factory ---------------------------------------------------------------

    /// <summary>
    /// Creates a new Intelligence.DataLake activity record and raises
    /// <see cref="DataLakeActivityCreatedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static DataLakeActivity Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string datasetName,
        string zone,
        long? recordCount,
        UserId processedBy)
    {
        var normalizedDatasetName = (datasetName ?? string.Empty).Trim();
        var normalizedZone = (zone ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedDatasetName))
            throw new ArgumentException("DatasetName is required.", nameof(datasetName));
        if (string.IsNullOrWhiteSpace(normalizedZone))
            throw new ArgumentException("Zone is required.", nameof(zone));
        if (processedBy == default)
            throw new ArgumentException("ProcessedBy is required.", nameof(processedBy));

        var agg = new DataLakeActivity
        {
            DataLakeActivityId = DataLakeActivityId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = DataLakeActivityStatus.Ingested,
            DatasetName = normalizedDatasetName,
            Zone = normalizedZone,
            RecordCount = recordCount,
            ProcessedAtUtc = createdAtUtc,
            ProcessedBy = processedBy,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DataLakeActivityCreatedEvent(
            EventId: DataLakeActivityEventId.New(),
            DataLakeActivityId: agg.DataLakeActivityId,
            TenantId: tenantId,
            SiteId: siteId,
            DatasetName: agg.DatasetName,
            Zone: agg.Zone,
            RecordCount: agg.RecordCount,
            ProcessedBy: processedBy,
            ProcessedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: StartProcessing -------------------------------------------

    /// <summary>
    /// Begins processing of the ingested data (Ingested -&gt; Processing). Valid only from
    /// <see cref="DataLakeActivityStatus.Ingested"/>.
    /// </summary>
    public void StartProcessing(
        UserId startedBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DataLakeActivityLifecycleRules.CanStartProcessing, DataLakeActivityStatus.Processing);

        ProcessingStartedBy = startedBy;
        ProcessingStartedAtUtc = startedAtUtc;
        Status = DataLakeActivityStatus.Processing;

        AddDomainEvent(new DataLakeActivityProcessingStartedEvent(
            EventId: DataLakeActivityEventId.New(),
            DataLakeActivityId: DataLakeActivityId,
            StartedBy: startedBy,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Curate -----------------------------------------------------

    /// <summary>
    /// Marks the activity's data as curated (Processing -&gt; Curated). Valid only from
    /// <see cref="DataLakeActivityStatus.Processing"/>. Updates <see cref="RecordCount"/> if
    /// <paramref name="finalRecordCount"/> is provided.
    /// </summary>
    public void Curate(
        long? finalRecordCount,
        UtcTimestamp curatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DataLakeActivityLifecycleRules.CanCurate, DataLakeActivityStatus.Curated);

        if (finalRecordCount.HasValue)
            RecordCount = finalRecordCount;

        CuratedAtUtc = curatedAtUtc;
        Status = DataLakeActivityStatus.Curated;

        AddDomainEvent(new DataLakeActivityCuratedEvent(
            EventId: DataLakeActivityEventId.New(),
            DataLakeActivityId: DataLakeActivityId,
            FinalRecordCount: RecordCount,
            CuratedAtUtc: curatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Fail ------------------------------------------------------

    /// <summary>
    /// Marks the activity as failed (Processing -&gt; Failed). Valid only from
    /// <see cref="DataLakeActivityStatus.Processing"/>.
    /// </summary>
    public void Fail(
        string failureReason,
        UtcTimestamp failedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DataLakeActivityLifecycleRules.CanFail, DataLakeActivityStatus.Failed);

        FailureReason = (failureReason ?? string.Empty).Trim();
        FailedAtUtc = failedAtUtc;
        Status = DataLakeActivityStatus.Failed;

        AddDomainEvent(new DataLakeActivityFailedEvent(
            EventId: DataLakeActivityEventId.New(),
            DataLakeActivityId: DataLakeActivityId,
            FailureReason: FailureReason,
            FailedAtUtc: failedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive ---------------------------------------------------------

    /// <summary>
    /// Archives a curated or failed activity for compliance retention (Curated OR Failed -&gt;
    /// Archived).
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DataLakeActivityLifecycleRules.CanArchive, DataLakeActivityStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = DataLakeActivityStatus.Archived;

        AddDomainEvent(new DataLakeActivityArchivedEvent(
            EventId: DataLakeActivityEventId.New(),
            DataLakeActivityId: DataLakeActivityId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDataLakeActivityDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<DataLakeActivityStatus, bool> guard, DataLakeActivityStatus expected)
    {
        if (guard(Status)) return;

        throw new DataLakeActivityInvalidStatusTransitionException(DataLakeActivityId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
