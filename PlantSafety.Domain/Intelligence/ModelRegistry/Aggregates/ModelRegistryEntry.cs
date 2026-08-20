// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntry.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Intelligence.ModelRegistry bounded context. Ported
//                  (rebuilt, not copy-pasted) from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/Entities/ModelRegistryEntry.cs -- which this
//                  whole subtree (Intelligence.Domain) is a confirmed-dead, unwired project slated
//                  for later deletion -- into this real, live Intelligence module's own
//                  conventions: strong-typed ModelRegistryEntryId (not a plain Guid Id on
//                  AggregateRoot&lt;Guid&gt;), tenant/site scoping, guarded-transition/domain-event/
//                  exception pattern, and PlantSafety.Domain.Intelligence.ModelRegistry.* namespace.
//                  Mirrors PlantSafety.Domain.Intelligence.DataLake.Aggregates.DataLakeActivity's
//                  identical precedent (itself modeled on
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA).
//
//                  SAFETY-CRITICAL (Safety Rule 4.5): only entries that reach
//                  ModelRegistryEntryStatus.Approved are deployment-eligible. See
//                  Services.ModelRegistryDomainService.IsEligibleForProductionDeployment for the
//                  fail-safe eligibility gate (also requires
//                  TrainingAccuracyScore &gt;= the module's MinimumConfidenceScore threshold) and
//                  SupersedesCurrentDeployment for the SemVer-based deployment supersession check.
//
//                  INVARIANTS (ported from the orphan's documented invariants):
//                    - Retired status is terminal.
//                    - Only Approved models may be deployed.
//                    - Approval requires an explicit human approver actor
//                      (ModelRegistryEntryApprovalRequiresHumanActorException).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.ModelRegistry.Enums;
using PlantSafety.Domain.Intelligence.ModelRegistry.Events;
using PlantSafety.Domain.Intelligence.ModelRegistry.Exceptions;
using PlantSafety.Domain.Intelligence.ModelRegistry.Rules;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates;

/// <summary>Aggregate root for the Intelligence.ModelRegistry bounded context.</summary>
public sealed class ModelRegistryEntry
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this ModelRegistryEntry aggregate.</summary>
    public ModelRegistryEntryId ModelRegistryEntryId { get; private set; }

    /// <summary>Tenant that owns this model registry entry.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current governance status.</summary>
    public ModelRegistryEntryStatus Status { get; private set; }

    // ---- ModelRegistryEntry-specific business properties ---------------------

    /// <summary>The logical model name.</summary>
    public string ModelName { get; private set; } = string.Empty;

    /// <summary>The semantic version of this model.</summary>
    public ModelVersion Version { get; private set; } = null!;

    /// <summary>SHA-256 hash of the training dataset. Used for lineage tracking.</summary>
    public string DatasetHash { get; private set; } = string.Empty;

    /// <summary>Primary accuracy score from the training job (0.0-1.0).</summary>
    public double TrainingAccuracyScore { get; private set; }

    /// <summary>The actor who registered this model.</summary>
    public UserId RegisteredBy { get; private set; }

    /// <summary>The human actor who approved this model. Null until approved.</summary>
    public UserId? ApprovedBy { get; private set; }

    /// <summary>UTC timestamp when approved. Null until approved.</summary>
    public UtcTimestamp? ApprovedAtUtc { get; private set; }

    /// <summary>Sanitized deprecation reason. Populated when deprecated.</summary>
    public string? DeprecationReason { get; private set; }

    /// <summary>UTC instant the entry was deprecated.</summary>
    public UtcTimestamp? DeprecatedAtUtc { get; private set; }

    /// <summary>UTC instant the entry was retired.</summary>
    public UtcTimestamp? RetiredAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user or process that registered this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IModelRegistryEntryDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IModelRegistryEntryDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // For rehydration only.
    private ModelRegistryEntry() { }

    // ---- Factory ---------------------------------------------------------------

    /// <summary>
    /// Registers a new model version in the registry and raises
    /// <see cref="ModelRegistryEntryRegisteredEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static ModelRegistryEntry Register(
        UserId registeredBy,
        UtcTimestamp registeredAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string modelName,
        ModelVersion version,
        string datasetHash,
        double trainingAccuracyScore)
    {
        ArgumentNullException.ThrowIfNull(version);

        var normalizedModelName = (modelName ?? string.Empty).Trim();
        var normalizedDatasetHash = (datasetHash ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedModelName))
            throw new ArgumentException("ModelName is required.", nameof(modelName));
        if (string.IsNullOrWhiteSpace(normalizedDatasetHash))
            throw new ArgumentException("DatasetHash is required.", nameof(datasetHash));
        if (trainingAccuracyScore is < 0.0 or > 1.0)
            throw new ArgumentException("TrainingAccuracyScore must be between 0.0 and 1.0.", nameof(trainingAccuracyScore));

        var agg = new ModelRegistryEntry
        {
            ModelRegistryEntryId = ModelRegistryEntryId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = ModelRegistryEntryStatus.Draft,
            ModelName = normalizedModelName,
            Version = version,
            DatasetHash = normalizedDatasetHash,
            TrainingAccuracyScore = trainingAccuracyScore,
            RegisteredBy = registeredBy,
            CreatedAtUtc = registeredAtUtc,
            CreatedBy = registeredBy,
        };

        agg.AddDomainEvent(new ModelRegistryEntryRegisteredEvent(
            EventId: ModelRegistryEntryEventId.New(),
            ModelRegistryEntryId: agg.ModelRegistryEntryId,
            TenantId: tenantId,
            SiteId: siteId,
            ModelName: agg.ModelName,
            ModelVersion: version.Value,
            DatasetHash: agg.DatasetHash,
            TrainingAccuracyScore: trainingAccuracyScore,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: SubmitForApproval -----------------------------------------

    /// <summary>Submits this model for human approval (Draft -&gt; PendingApproval).</summary>
    public void SubmitForApproval(PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ModelRegistryEntryLifecycleRules.CanSubmitForApproval, ModelRegistryEntryStatus.PendingApproval);

        Status = ModelRegistryEntryStatus.PendingApproval;

        AddDomainEvent(new ModelRegistryEntrySubmittedForApprovalEvent(
            EventId: ModelRegistryEntryEventId.New(),
            ModelRegistryEntryId: ModelRegistryEntryId,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Approve -----------------------------------------------------

    /// <summary>
    /// Approves this model for production deployment (PendingApproval -&gt; Approved).
    /// SAFETY-CRITICAL (Safety Rule 4.5): <paramref name="approvedBy"/> must be a human user actor
    /// -- never a system or AI-agent identity.
    /// </summary>
    /// <exception cref="ModelRegistryEntryApprovalRequiresHumanActorException">
    /// Thrown when <paramref name="actor"/> is not a human user actor.
    /// </exception>
    public void Approve(UserId approvedBy, PerformedByActor actor, UtcTimestamp approvedAtUtc, CorrelationId correlationId, CausationId causationId)
    {
        if (actor.Kind != ActorKind.User)
            throw new ModelRegistryEntryApprovalRequiresHumanActorException(ModelRegistryEntryId, correlationId, causationId);

        EnsureStatusIsOneOf(correlationId, causationId, ModelRegistryEntryLifecycleRules.CanApprove, ModelRegistryEntryStatus.Approved);

        Status = ModelRegistryEntryStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;

        AddDomainEvent(new ModelRegistryEntryApprovedEvent(
            EventId: ModelRegistryEntryEventId.New(),
            ModelRegistryEntryId: ModelRegistryEntryId,
            ModelName: ModelName,
            ModelVersion: Version.Value,
            ApprovedByActorId: approvedBy,
            ApprovedAtUtc: approvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Deprecate ---------------------------------------------------

    /// <summary>Deprecates this model, typically when superseded by a newer approved version (Approved -&gt; Deprecated).</summary>
    public void Deprecate(string reason, UtcTimestamp deprecatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ModelRegistryEntryLifecycleRules.CanDeprecate, ModelRegistryEntryStatus.Deprecated);

        DeprecationReason = (reason ?? string.Empty).Trim();
        DeprecatedAtUtc = deprecatedAtUtc;
        Status = ModelRegistryEntryStatus.Deprecated;

        AddDomainEvent(new ModelRegistryEntryDeprecatedEvent(
            EventId: ModelRegistryEntryEventId.New(),
            ModelRegistryEntryId: ModelRegistryEntryId,
            ModelName: ModelName,
            ModelVersion: Version.Value,
            DeprecationReason: DeprecationReason,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Retire -------------------------------------------------------

    /// <summary>Permanently retires this model. Terminal -- no further transitions are permitted.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ModelRegistryEntryLifecycleRules.CanRetire, ModelRegistryEntryStatus.Retired);

        RetiredAtUtc = retiredAtUtc;
        Status = ModelRegistryEntryStatus.Retired;

        AddDomainEvent(new ModelRegistryEntryRetiredEvent(
            EventId: ModelRegistryEntryEventId.New(),
            ModelRegistryEntryId: ModelRegistryEntryId,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IModelRegistryEntryDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ModelRegistryEntryStatus, bool> guard, ModelRegistryEntryStatus expected)
    {
        if (guard(Status)) return;

        throw new ModelRegistryEntryInvalidStatusTransitionException(ModelRegistryEntryId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
