// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           Policy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Aggregates;

/// <summary>Aggregate root for the Policy bounded context.</summary>
public sealed partial class Policy
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Policy aggregate.</summary>
    public PolicyId PolicyId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PolicyStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPolicyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPolicyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPolicyDomainEvent>(_domainEvents);

    // For rehydration only.
    private Policy() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Policy aggregate and raises <see cref="PolicyCreatedEvent"/>.</summary>
    public static Policy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Policy
        {
            PolicyId    = PolicyId.New(),
            Status      = PolicyStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new PolicyCreatedEvent(
            EventId:       new PolicyEventId(Uuid7.New()),
            PolicyId:      agg.PolicyId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPolicyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PolicyStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Policy {PolicyId}.");
    }
}
