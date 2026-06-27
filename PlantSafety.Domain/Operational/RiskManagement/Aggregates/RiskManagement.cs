// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagement.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Aggregates;

/// <summary>Aggregate root for the RiskManagement bounded context.</summary>
public sealed partial class RiskManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this RiskManagement aggregate.</summary>
    public RiskManagementId RiskManagementId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RiskManagementStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRiskManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRiskManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRiskManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private RiskManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new RiskManagement aggregate and raises <see cref="RiskManagementCreatedEvent"/>.</summary>
    public static RiskManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new RiskManagement
        {
            RiskManagementId    = RiskManagementId.New(),
            Status      = RiskManagementStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new RiskManagementCreatedEvent(
            EventId:       new RiskManagementEventId(Uuid7.New()),
            RiskManagementId:      agg.RiskManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRiskManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RiskManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for RiskManagement {RiskManagementId}.");
    }
}
