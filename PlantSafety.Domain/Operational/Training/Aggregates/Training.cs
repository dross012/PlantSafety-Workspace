// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           Training.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Aggregates;

/// <summary>Aggregate root for the Training bounded context.</summary>
public sealed partial class Training
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Training aggregate.</summary>
    public TrainingId TrainingId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TrainingStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITrainingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITrainingDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITrainingDomainEvent>(_domainEvents);

    // For rehydration only.
    private Training() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Training aggregate and raises <see cref="TrainingCreatedEvent"/>.</summary>
    public static Training Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Training
        {
            TrainingId    = TrainingId.New(),
            Status      = TrainingStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new TrainingCreatedEvent(
            EventId:       new TrainingEventId(Uuid7.New()),
            TrainingId:      agg.TrainingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITrainingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TrainingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Training {TrainingId}.");
    }
}
