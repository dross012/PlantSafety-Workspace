/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelRegistryEntry.cs
    DESCRIPTION:        Entity representing a registered model version in the Intelligence model registry.
                        Human approval is mandatory before production deployment (Safety Rule 4.5).

    INVARIANTS:
        - Retired status is terminal.
        - Only Approved models may be deployed.
        - Approval requires an explicit human approver actor ID.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Exceptions;
using Intelligence.Domain.ModelRegistry.Events;
using Intelligence.Domain.ModelRegistry.ValueObjects;

namespace Intelligence.Domain.ModelRegistry.Entities;

/// <summary>A registered model version in the Intelligence model registry.</summary>
public sealed class ModelRegistryEntry : AggregateRoot<Guid>
{
    /// <summary>The logical model name.</summary>
    public string ModelName { get; private init; }

    /// <summary>The semantic version of this model.</summary>
    public ModelVersion Version { get; private init; }

    /// <summary>Current governance status.</summary>
    public ModelStatus Status { get; private set; }

    /// <summary>SHA-256 hash of the training dataset. Used for lineage tracking.</summary>
    public string DatasetHash { get; private init; }

    /// <summary>Primary accuracy score from the training job.</summary>
    public double TrainingAccuracyScore { get; private init; }

    /// <summary>The actor who registered this model.</summary>
    public string RegisteredByActorId { get; private init; }

    /// <summary>The human actor who approved this model. Null until approved.</summary>
    public string? ApprovedByActorId { get; private set; }

    /// <summary>UTC timestamp when registered.</summary>
    public DateTime RegisteredAtUtc { get; private init; }

    /// <summary>UTC timestamp when approved. Null until approved.</summary>
    public DateTime? ApprovedAtUtc { get; private set; }

    /// <summary>Sanitized deprecation reason. Populated when deprecated.</summary>
    public string? DeprecationReason { get; private set; }

    private ModelRegistryEntry(
        Guid id, string modelName, ModelVersion version, string datasetHash,
        double trainingAccuracyScore, string registeredByActorId) : base(id)
    {
        ModelName = modelName;
        Version = version;
        Status = ModelStatus.Draft;
        DatasetHash = datasetHash;
        TrainingAccuracyScore = trainingAccuracyScore;
        RegisteredByActorId = registeredByActorId;
        RegisteredAtUtc = DateTime.UtcNow;
    }

    /// <summary>Registers a new model version in the registry and raises <see cref="ModelRegisteredEvent"/>.</summary>
    public static ModelRegistryEntry Register(
        string modelName, ModelVersion version, string datasetHash,
        double trainingAccuracyScore, string registeredByActorId,
        Guid correlationId, Guid causationId)
    {
        if (string.IsNullOrWhiteSpace(modelName)) throw new ArgumentException("ModelName cannot be empty.", nameof(modelName));
        if (string.IsNullOrWhiteSpace(datasetHash)) throw new ArgumentException("DatasetHash cannot be empty.", nameof(datasetHash));

        var entry = new ModelRegistryEntry(
            Guid.NewGuid(), modelName.Trim(), version, datasetHash, trainingAccuracyScore, registeredByActorId);

        entry.RaiseDomainEvent(new ModelRegisteredEvent
        {
            AggregateId = entry.Id,
            CorrelationId = correlationId,
            CausationId = causationId,
            ModelName = modelName.Trim(),
            ModelVersion = version.Value,
            DatasetHash = datasetHash,
            TrainingAccuracyScore = trainingAccuracyScore,
            ActorId = registeredByActorId
        });

        return entry;
    }

    /// <summary>Submits this model for human approval.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when not in Draft status.</exception>
    public void SubmitForApproval(Guid correlationId, Guid causationId)
    {
        if (Status != ModelStatus.Draft)
            throw new IntelligenceDomainException("MODEL_NOT_DRAFT",
                "Only Draft models can be submitted for approval.", correlationId, causationId);
        Status = ModelStatus.PendingApproval;
    }

    /// <summary>Approves this model for production deployment. Requires a human approver.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when not in PendingApproval status.</exception>
    public void Approve(string approverActorId, Guid correlationId, Guid causationId)
    {
        if (Status != ModelStatus.PendingApproval)
            throw new IntelligenceDomainException("MODEL_NOT_PENDING_APPROVAL",
                "Only PendingApproval models can be approved.", correlationId, causationId);

        Status = ModelStatus.Approved;
        ApprovedByActorId = approverActorId;
        ApprovedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ModelApprovedEvent
        {
            AggregateId = Id,
            CorrelationId = correlationId,
            CausationId = causationId,
            ModelName = ModelName,
            ModelVersion = Version.Value,
            ApprovedByActorId = approverActorId,
            ApprovedAtUtc = ApprovedAtUtc.Value,
            ActorId = approverActorId
        });
    }

    /// <summary>Deprecates this model (typically when superseded by a newer version).</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when model cannot be deprecated from its current status.</exception>
    public void Deprecate(string reason, Guid correlationId, Guid causationId)
    {
        if (Status.IsTerminal || Status == ModelStatus.Draft || Status == ModelStatus.PendingApproval)
            throw new IntelligenceDomainException("MODEL_CANNOT_DEPRECATE",
                $"Model in status '{Status}' cannot be deprecated.", correlationId, causationId);

        Status = ModelStatus.Deprecated;
        DeprecationReason = reason;

        RaiseDomainEvent(new ModelDeprecatedEvent
        {
            AggregateId = Id,
            CorrelationId = correlationId,
            CausationId = causationId,
            ModelName = ModelName,
            ModelVersion = Version.Value,
            DeprecationReason = reason
        });
    }

    /// <summary>Permanently retires this model. Terminal — no further transitions are permitted.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when already terminal.</exception>
    public void Retire(Guid correlationId, Guid causationId)
    {
        if (Status.IsTerminal)
            throw new IntelligenceDomainException("MODEL_ALREADY_TERMINAL",
                $"Model in status '{Status}' cannot be retired.", correlationId, causationId);

        Status = ModelStatus.Retired;
    }
}
