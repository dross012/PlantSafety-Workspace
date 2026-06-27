/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJob.cs
    DESCRIPTION:        Aggregate Root for the AITraining module. Manages ML training job lifecycle:
                        submission → dataset registration → start → complete/fail/cancel.

    INVARIANTS:
        - Cannot start without a registered dataset.
        - Cannot complete without a model accuracy score.
        - Terminal jobs cannot transition further.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Exceptions;
using Intelligence.Domain.AITraining.Entities;
using Intelligence.Domain.AITraining.Events;
using Intelligence.Domain.AITraining.ValueObjects;

namespace Intelligence.Domain.AITraining.Aggregates;

/// <summary>Aggregate root for a machine learning training job.</summary>
public sealed class TrainingJob : AggregateRoot<TrainingJobId>
{
    /// <summary>The model name being trained.</summary>
    public string ModelName { get; private init; }

    /// <summary>The target model version.</summary>
    public string ModelVersion { get; private init; }

    /// <summary>Current lifecycle status.</summary>
    public TrainingJobStatus Status { get; private set; }

    /// <summary>The registered dataset. Null until RegisterDataset is called.</summary>
    public TrainingDataset? Dataset { get; private set; }

    /// <summary>The model accuracy score after evaluation. Null until Complete is called.</summary>
    public ModelAccuracyScore? AccuracyScore { get; private set; }

    /// <summary>UTC timestamp when this job was submitted.</summary>
    public DateTime SubmittedAtUtc { get; private init; }

    /// <summary>UTC timestamp when compute resources were allocated.</summary>
    public DateTime? StartedAtUtc { get; private set; }

    /// <summary>UTC timestamp when this job reached a terminal state.</summary>
    public DateTime? FinishedAtUtc { get; private set; }

    /// <summary>The actor who submitted this job.</summary>
    public string SubmittedByActorId { get; private init; }

    /// <summary>Sanitized failure reason. Populated only on failure.</summary>
    public string? FailureReason { get; private set; }

    private TrainingJob(TrainingJobId id, string modelName, string modelVersion, string submittedByActorId)
        : base(id)
    {
        ModelName = modelName;
        ModelVersion = modelVersion;
        SubmittedByActorId = submittedByActorId;
        Status = TrainingJobStatus.Pending;
        SubmittedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Submits a new training job.</summary>
    public static TrainingJob Submit(
        string modelName, string modelVersion, string submittedByActorId, Guid correlationId, Guid causationId)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException("ModelName cannot be empty.", nameof(modelName));
        if (string.IsNullOrWhiteSpace(modelVersion))
            throw new ArgumentException("ModelVersion cannot be empty.", nameof(modelVersion));

        var id = TrainingJobId.New();
        var job = new TrainingJob(id, modelName.Trim(), modelVersion.Trim(), submittedByActorId);

        job.RaiseDomainEvent(new TrainingJobStartedEvent
        {
            AggregateId = id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            ModelName = modelName.Trim(),
            ModelVersion = modelVersion.Trim(),
            ActorId = submittedByActorId
        });

        return job;
    }

    /// <summary>Registers the training dataset. Must be called before Start.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when job is terminal or dataset already registered.</exception>
    public void RegisterDataset(TrainingDataset dataset, Guid correlationId, Guid causationId)
    {
        if (Status.IsTerminal)
            throw new IntelligenceDomainException("TRAINING_JOB_TERMINAL",
                "Cannot register a dataset on a terminal training job.", correlationId, causationId);
        if (Dataset is not null)
            throw new IntelligenceDomainException("TRAINING_DATASET_ALREADY_REGISTERED",
                "A dataset is already registered for this training job.", correlationId, causationId);

        Dataset = dataset;
    }

    /// <summary>Marks the job as started (compute allocated).</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when no dataset is registered or job is not pending.</exception>
    public void Start(Guid correlationId, Guid causationId)
    {
        if (Status != TrainingJobStatus.Pending)
            throw new IntelligenceDomainException("TRAINING_JOB_NOT_PENDING",
                "Only pending training jobs can be started.", correlationId, causationId);
        if (Dataset is null)
            throw new IntelligenceDomainException("TRAINING_DATASET_REQUIRED",
                "A dataset must be registered before a training job can start.", correlationId, causationId);

        Status = TrainingJobStatus.Running;
        StartedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Marks the job as successfully completed with an accuracy score.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when job is not running.</exception>
    public void Complete(ModelAccuracyScore accuracyScore, Guid correlationId, Guid causationId)
    {
        if (!Status.IsRunning)
            throw new IntelligenceDomainException("TRAINING_JOB_NOT_RUNNING",
                "Only running training jobs can be completed.", correlationId, causationId);

        Status = TrainingJobStatus.Completed;
        AccuracyScore = accuracyScore;
        FinishedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new TrainingJobCompletedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            ModelName = ModelName,
            ModelVersion = ModelVersion,
            AccuracyScore = accuracyScore.Score,
            MetricName = accuracyScore.MetricName,
            DatasetHash = Dataset?.Hash.Value ?? string.Empty,
            MeetsDeploymentThreshold = accuracyScore.MeetsDeploymentThreshold
        });
    }

    /// <summary>
    /// Transitions the job from Running to Evaluating (compute metrics phase).
    /// Called by the Azure ML adapter when the training run finishes and evaluation begins.
    /// </summary>
    /// <exception cref="IntelligenceDomainException">Thrown when not in Running status.</exception>
    public void BeginEvaluation(Guid correlationId, Guid causationId)
    {
        if (Status != TrainingJobStatus.Running)
            throw new IntelligenceDomainException("TRAINING_JOB_NOT_RUNNING",
                "Only running training jobs can transition to evaluation.", correlationId, causationId);

        Status = TrainingJobStatus.Evaluating;
    }

    /// <summary>Marks the job as failed with a sanitized reason.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when already terminal.</exception>
    public void Fail(string sanitizedReason, Guid correlationId, Guid causationId)
    {
        if (Status.IsTerminal)
            throw new IntelligenceDomainException("TRAINING_JOB_ALREADY_TERMINAL",
                "A terminal training job cannot be failed.", correlationId, causationId);

        Status = TrainingJobStatus.Failed;
        FailureReason = sanitizedReason;
        FinishedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new TrainingJobFailedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            ModelName = ModelName,
            SanitizedReason = sanitizedReason
        });
    }

    /// <summary>Cancels the training job before it reaches a terminal state.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when already terminal.</exception>
    public void Cancel(Guid correlationId, Guid causationId)
    {
        if (Status.IsTerminal)
            throw new IntelligenceDomainException("TRAINING_JOB_ALREADY_TERMINAL",
                "A terminal training job cannot be cancelled.", correlationId, causationId);

        Status = TrainingJobStatus.Cancelled;
        FinishedAtUtc = DateTime.UtcNow;
    }
}
