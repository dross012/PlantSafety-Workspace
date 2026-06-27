// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitor.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ContractorVisitor module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Aggregates;

/// <summary>Aggregate root for the ContractorVisitor bounded context.</summary>
public sealed class ContractorVisitor
{
    /// <summary>Unique identity of this ContractorVisitor aggregate.</summary>
    public ContractorVisitorId ContractorVisitorId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IContractorVisitorDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IContractorVisitorDomainEvent> DomainEvents
        => new ReadOnlyCollection<IContractorVisitorDomainEvent>(_domainEvents);

    private ContractorVisitor() { }

    /// <summary>Creates a new ContractorVisitor aggregate and raises <see cref="ContractorVisitorCreatedEvent"/>.</summary>
    public static ContractorVisitor Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new ContractorVisitor
        {
            ContractorVisitorId     = ContractorVisitorId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new ContractorVisitorCreatedEvent(
            EventId:       Uuid7.New(),
            ContractorVisitorId:      agg.ContractorVisitorId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
