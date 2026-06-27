// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           Compliance.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Aggregates;

/// <summary>Aggregate root for the Compliance bounded context.</summary>
public sealed partial class Compliance
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Compliance aggregate.</summary>
    public ComplianceId ComplianceId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ComplianceStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IComplianceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IComplianceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IComplianceDomainEvent>(_domainEvents);

    // For rehydration only.
    private Compliance() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Compliance aggregate and raises <see cref="ComplianceCreatedEvent"/>.</summary>
    public static Compliance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Compliance
        {
            ComplianceId    = ComplianceId.New(),
            Status      = ComplianceStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ComplianceCreatedEvent(
            EventId:       new ComplianceEventId(Uuid7.New()),
            ComplianceId:      agg.ComplianceId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IComplianceDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ComplianceStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Compliance {ComplianceId}.");
    }
}
