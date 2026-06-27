// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           Contractor.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Aggregates;

/// <summary>Aggregate root for the Contractor bounded context.</summary>
public sealed partial class Contractor
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Contractor aggregate.</summary>
    public ContractorId ContractorId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ContractorStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IContractorDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IContractorDomainEvent> DomainEvents
        => new ReadOnlyCollection<IContractorDomainEvent>(_domainEvents);

    // For rehydration only.
    private Contractor() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Contractor aggregate and raises <see cref="ContractorCreatedEvent"/>.</summary>
    public static Contractor Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Contractor
        {
            ContractorId    = ContractorId.New(),
            Status      = ContractorStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ContractorCreatedEvent(
            EventId:       new ContractorEventId(Uuid7.New()),
            ContractorId:      agg.ContractorId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IContractorDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ContractorStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Contractor {ContractorId}.");
    }
}
