// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           Procedures.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Aggregates;

/// <summary>Aggregate root for the Procedures bounded context.</summary>
public sealed partial class Procedures
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Procedures aggregate.</summary>
    public ProceduresId ProceduresId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ProceduresStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IProceduresDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IProceduresDomainEvent> DomainEvents
        => new ReadOnlyCollection<IProceduresDomainEvent>(_domainEvents);

    // For rehydration only.
    private Procedures() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Procedures aggregate and raises <see cref="ProceduresCreatedEvent"/>.</summary>
    public static Procedures Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Procedures
        {
            ProceduresId    = ProceduresId.New(),
            Status      = ProceduresStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ProceduresCreatedEvent(
            EventId:       new ProceduresEventId(Uuid7.New()),
            ProceduresId:      agg.ProceduresId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IProceduresDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ProceduresStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Procedures {ProceduresId}.");
    }
}
