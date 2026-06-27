// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           Certification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Aggregates;

/// <summary>Aggregate root for the Certification bounded context.</summary>
public sealed partial class Certification
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Certification aggregate.</summary>
    public CertificationId CertificationId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CertificationStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICertificationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICertificationDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICertificationDomainEvent>(_domainEvents);

    // For rehydration only.
    private Certification() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Certification aggregate and raises <see cref="CertificationCreatedEvent"/>.</summary>
    public static Certification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Certification
        {
            CertificationId    = CertificationId.New(),
            Status      = CertificationStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new CertificationCreatedEvent(
            EventId:       new CertificationEventId(Uuid7.New()),
            CertificationId:      agg.CertificationId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICertificationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CertificationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Certification {CertificationId}.");
    }
}
