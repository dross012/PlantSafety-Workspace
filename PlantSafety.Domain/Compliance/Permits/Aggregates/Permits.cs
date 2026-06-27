// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           Permits.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Aggregates;

/// <summary>Aggregate root for the Permits bounded context.</summary>
public sealed partial class Permits
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Permits aggregate.</summary>
    public PermitsId PermitsId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitsStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitsDomainEvent>(_domainEvents);

    // For rehydration only.
    private Permits() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Permits aggregate and raises <see cref="PermitsCreatedEvent"/>.</summary>
    public static Permits Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Permits
        {
            PermitsId    = PermitsId.New(),
            Status      = PermitsStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new PermitsCreatedEvent(
            EventId:       new PermitsEventId(Uuid7.New()),
            PermitsId:      agg.PermitsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Permits {PermitsId}.");
    }
}
