// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJob.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Intelligence.AITraining bounded context, backing
//                  [Intelligence].[AITrainingJob] (usp_AITrainingJob_Insert /
//                  usp_AITrainingJob_Update / usp_AITrainingJob_GetById via the Domain-layer
//                  IAITrainingJobRepository / AITrainingJobAggregateDapperRepository). Before this
//                  pass, this module contained only an "AITrainingStub.cs" placeholder marker class
//                  ("Replace with the full aggregate structure") -- there was no domain model at
//                  all backing this feature; only a read-only date-range list query
//                  (PlantSafety.Application.Intelligence.AITraining.Queries.GetAITrainingQuery via
//                  PlantSafety.Application.Intelligence.AITraining.Repositories.IAITrainingRepository
//                  / PlantSafety.Infrastructure.Intelligence.AITraining.Persistence
//                  .AITrainingDapperRepository calling usp_AITraining_GetTrainingJobs) existed, and
//                  it is left entirely untouched by this pass -- it is a different, coarser
//                  reporting query, not the by-id/lifecycle path built here. Field shape
//                  (TrainingJobId, TenantId, SiteId, ModelName, ModelVersion, DatasetReference,
//                  Status, StartedAtUtc, InitiatedBy) is grounded in that pre-existing read model's
//                  actual DTO (PlantSafety.Application.Intelligence.AITraining.Models
//                  .AITrainingItemDto), not invented from scratch. Lifecycle states
//                  (Queued/Running/Completed/Failed) are taken from AITrainingItemDto.Status's
//                  documented vocabulary plus the platform-standard terminal Archived state.
//                  Mirrors PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA's
//                  guarded-transition/domain-event/exception conventions (EnsureStatusIsOneOf +
//                  AITrainingJobLifecycleRules.CanX guard pattern).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AITraining.Enums;
using PlantSafety.Domain.Intelligence.AITraining.Events;
using PlantSafety.Domain.Intelligence.AITraining.Exceptions;
using PlantSafety.Domain.Intelligence.AITraining.Rules;
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.AITraining.Aggregates;

/// <summary>Aggregate root for the Intelligence.AITraining bounded context.</summary>
public sealed class AITrainingJob
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this AITrainingJob aggregate.</summary>
    public AITrainingJobId AITrainingJobId { get; private set; }

    /// <summary>Tenant that owns this training job record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AITrainingJobStatus Status { get; private set; }

    // ---- AITrainingJob-specific business properties ------------------------

    /// <summary>Name of the model being trained.</summary>
    public string ModelName { get; private set; } = string.Empty;

    /// <summary>Version identifier of the trained model artifact.</summary>
    public string ModelVersion { get; private set; } = string.Empty;

    /// <summary>Reference to the training dataset used for this run.</summary>
    public string? DatasetReference { get; private set; }

    /// <summary>UTC timestamp when the training job started (or was queued to start).</summary>
    public UtcTimestamp StartedAtUtc { get; private set; }

    /// <summary>Identity of the user or process that initiated the training job.</summary>
    public UserId InitiatedBy { get; private set; }

    // ---- Lifecycle attributes (Start/Complete/Fail/Archive) -----------------

    /// <summary>Identity of the user who started (transitioned to Running) the training job.</summary>
    public UserId? RunStartedBy { get; private set; }

    /// <summary>UTC instant the training job transitioned to Running.</summary>
    public UtcTimestamp? RunStartedAtUtc { get; private set; }

    /// <summary>Reference to the produced model artifact, if any, recorded on completion.</summary>
    public string? ModelArtifactReference { get; private set; }

    /// <summary>UTC instant the training job completed successfully.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Failure reason recorded when the training job failed, if any.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>UTC instant the training job failed.</summary>
    public UtcTimestamp? FailedAtUtc { get; private set; }

    /// <summary>UTC instant the training job was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IAITrainingJobDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAITrainingJobDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // For rehydration only.
    private AITrainingJob() { }

    // ---- Factory ---------------------------------------------------------------

    /// <summary>
    /// Creates a new Intelligence.AITraining training job record and raises
    /// <see cref="AITrainingJobCreatedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static AITrainingJob Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string modelName,
        string modelVersion,
        string? datasetReference,
        UserId initiatedBy)
    {
        var normalizedModelName = (modelName ?? string.Empty).Trim();
        var normalizedModelVersion = (modelVersion ?? string.Empty).Trim();
        var normalizedDatasetReference = string.IsNullOrWhiteSpace(datasetReference) ? null : datasetReference.Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedModelName))
            throw new ArgumentException("ModelName is required.", nameof(modelName));
        if (string.IsNullOrWhiteSpace(normalizedModelVersion))
            throw new ArgumentException("ModelVersion is required.", nameof(modelVersion));
        if (initiatedBy == default)
            throw new ArgumentException("InitiatedBy is required.", nameof(initiatedBy));

        var agg = new AITrainingJob
        {
            AITrainingJobId = AITrainingJobId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = AITrainingJobStatus.Queued,
            ModelName = normalizedModelName,
            ModelVersion = normalizedModelVersion,
            DatasetReference = normalizedDatasetReference,
            StartedAtUtc = createdAtUtc,
            InitiatedBy = initiatedBy,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new AITrainingJobCreatedEvent(
            EventId: AITrainingJobEventId.New(),
            AITrainingJobId: agg.AITrainingJobId,
            TenantId: tenantId,
            SiteId: siteId,
            ModelName: agg.ModelName,
            ModelVersion: agg.ModelVersion,
            DatasetReference: agg.DatasetReference,
            InitiatedBy: initiatedBy,
            StartedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: Start ---------------------------------------------------

    /// <summary>
    /// Begins execution of the training job (Queued -&gt; Running). Valid only from
    /// <see cref="AITrainingJobStatus.Queued"/>.
    /// </summary>
    public void Start(
        UserId startedBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AITrainingJobLifecycleRules.CanStart, AITrainingJobStatus.Running);

        RunStartedBy = startedBy;
        RunStartedAtUtc = startedAtUtc;
        Status = AITrainingJobStatus.Running;

        AddDomainEvent(new AITrainingJobStartedEvent(
            EventId: AITrainingJobEventId.New(),
            AITrainingJobId: AITrainingJobId,
            StartedBy: startedBy,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete -------------------------------------------------

    /// <summary>
    /// Marks the training job as completed successfully (Running -&gt; Completed). Valid only from
    /// <see cref="AITrainingJobStatus.Running"/>.
    /// </summary>
    public void Complete(
        string? modelArtifactReference,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AITrainingJobLifecycleRules.CanComplete, AITrainingJobStatus.Completed);

        ModelArtifactReference = string.IsNullOrWhiteSpace(modelArtifactReference) ? null : modelArtifactReference.Trim();
        CompletedAtUtc = completedAtUtc;
        Status = AITrainingJobStatus.Completed;

        AddDomainEvent(new AITrainingJobCompletedEvent(
            EventId: AITrainingJobEventId.New(),
            AITrainingJobId: AITrainingJobId,
            ModelArtifactReference: ModelArtifactReference,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Fail ------------------------------------------------------

    /// <summary>
    /// Marks the training job as failed (Running -&gt; Failed). Valid only from
    /// <see cref="AITrainingJobStatus.Running"/>.
    /// </summary>
    public void Fail(
        string failureReason,
        UtcTimestamp failedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AITrainingJobLifecycleRules.CanFail, AITrainingJobStatus.Failed);

        FailureReason = (failureReason ?? string.Empty).Trim();
        FailedAtUtc = failedAtUtc;
        Status = AITrainingJobStatus.Failed;

        AddDomainEvent(new AITrainingJobFailedEvent(
            EventId: AITrainingJobEventId.New(),
            AITrainingJobId: AITrainingJobId,
            FailureReason: FailureReason,
            FailedAtUtc: failedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive ---------------------------------------------------------

    /// <summary>
    /// Archives a completed or failed training job for compliance retention
    /// (Completed OR Failed -&gt; Archived).
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AITrainingJobLifecycleRules.CanArchive, AITrainingJobStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = AITrainingJobStatus.Archived;

        AddDomainEvent(new AITrainingJobArchivedEvent(
            EventId: AITrainingJobEventId.New(),
            AITrainingJobId: AITrainingJobId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAITrainingJobDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<AITrainingJobStatus, bool> guard, AITrainingJobStatus expected)
    {
        if (guard(Status)) return;

        throw new AITrainingJobInvalidStatusTransitionException(AITrainingJobId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
