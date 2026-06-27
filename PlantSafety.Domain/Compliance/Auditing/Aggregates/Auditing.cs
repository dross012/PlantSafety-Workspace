// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           Auditing.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Aggregates;

/// <summary>Aggregate root for the Auditing bounded context.</summary>
public sealed partial class Auditing
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Auditing aggregate.</summary>
    public AuditingId AuditingId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AuditingStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAuditingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAuditingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAuditingDomainEvent>(_domainEvents);

    // For rehydration only.
    private Auditing() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Auditing aggregate and raises <see cref="AuditingCreatedEvent"/>.</summary>
    public static Auditing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Auditing
        {
            AuditingId    = AuditingId.New(),
            Status      = AuditingStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new AuditingCreatedEvent(
            EventId:       new AuditingEventId(Uuid7.New()),
            AuditingId:      agg.AuditingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAuditingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AuditingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Auditing {AuditingId}.");
    }
}
