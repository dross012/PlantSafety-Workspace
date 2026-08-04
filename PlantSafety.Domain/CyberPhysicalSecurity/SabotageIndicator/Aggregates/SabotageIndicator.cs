// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicator.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SabotageIndicator bounded context (Sabotage Indicator
//                  Correlation subdomain of CyberPhysicalSecurity). Represents a single indication
//                  of possible coordinated cyber-physical sabotage, sourced from an unauthorized
//                  control change, a failed sensor-integrity check, a physical-access anomaly
//                  (future PhysicalAccessControl integration point), external threat intel, or
//                  manual analyst input.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Exceptions;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates;

/// <summary>Aggregate root for the SabotageIndicator bounded context.</summary>
public sealed partial class SabotageIndicator
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this SabotageIndicator aggregate.</summary>
    public SabotageIndicatorId SabotageIndicatorId { get; private set; }

    /// <summary>Tenant that owns this indicator.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the indicator relates to.</summary>
    public Guid SiteId { get; private set; }

    // ---- Indicator details --------------------------------------------------------

    /// <summary>Origin subsystem that produced this indicator.</summary>
    public SabotageIndicatorSource Source { get; private set; }

    /// <summary>Identity of the originating record (ControlChangeEventId, SensorIntegrityCheckId, etc.), if applicable.</summary>
    public Guid? SourceReferenceId { get; private set; }

    /// <summary>Human-readable description of the indicator.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Severity rating.</summary>
    public SabotageIndicatorSeverity Severity { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SabotageIndicatorStatus Status { get; private set; }

    /// <summary>Count of distinct-source indicators correlated with this one within the correlation window.</summary>
    public int CorrelatedIndicatorCount { get; private set; }

    /// <summary>UTC timestamp this indicator was raised.</summary>
    public UtcTimestamp RaisedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that raised this indicator (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ISabotageIndicatorDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISabotageIndicatorDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISabotageIndicatorDomainEvent>(_domainEvents);

    // For rehydration only.
    private SabotageIndicator() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Raises a new SabotageIndicator and raises <see cref="SabotageIndicatorRaisedEvent"/>.</summary>
    public static SabotageIndicator Raise(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        SabotageIndicatorSource source,
        string description,
        SabotageIndicatorSeverity severity,
        Guid? sourceReferenceId = null)
    {
        var agg = new SabotageIndicator
        {
            SabotageIndicatorId      = SabotageIndicatorId.New(),
            TenantId                  = tenantId,
            SiteId                    = siteId,
            Source                    = source,
            SourceReferenceId         = sourceReferenceId,
            Description               = (description ?? string.Empty).Trim(),
            Severity                  = severity,
            Status                    = SabotageIndicatorStatus.Open,
            CorrelatedIndicatorCount  = 1,
            RaisedAtUtc               = createdAtUtc,
            CreatedAtUtc              = createdAtUtc,
            CreatedBy                 = createdBy,
        };

        agg.AddDomainEvent(new SabotageIndicatorRaisedEvent(
            EventId:              SabotageIndicatorEventId.New(),
            SabotageIndicatorId:  agg.SabotageIndicatorId,
            Source:               source,
            Severity:             severity,
            RaisedAtUtc:          createdAtUtc,
            Actor:                actor,
            CorrelationId:        correlationId,
            CausationId:          causationId));

        return agg;
    }

    // ---- Behavior ------------------------------------------------------------

    /// <summary>
    /// Applies a correlation update from the SabotageCorrelationService: increments the correlated
    /// count and, if the multi-source auto-escalation threshold is met, raises
    /// <see cref="SabotageIndicatorEscalatedEvent"/> and promotes severity to Critical.
    /// </summary>
    public void ApplyCorrelation(int correlatedIndicatorCount, bool autoEscalate, UtcTimestamp asOfUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        CorrelatedIndicatorCount = correlatedIndicatorCount;

        if (!autoEscalate)
            return;

        Severity = SabotageIndicatorSeverity.Critical;
        Status = SabotageIndicatorStatus.UnderInvestigation;

        AddDomainEvent(new SabotageIndicatorEscalatedEvent(
            EventId:                  SabotageIndicatorEventId.New(),
            SabotageIndicatorId:      SabotageIndicatorId,
            CorrelatedIndicatorCount: correlatedIndicatorCount,
            EscalatedAtUtc:           asOfUtc,
            Actor:                    actor,
            CorrelationId:            correlationId,
            CausationId:              causationId));
    }

    /// <summary>Confirms this indicator as a genuine sabotage indication.</summary>
    public void Confirm()
    {
        EnsureStatusIsOneOf(SabotageIndicatorStatus.Open, SabotageIndicatorStatus.UnderInvestigation);
        Status = SabotageIndicatorStatus.Confirmed;
    }

    /// <summary>Marks this indicator as a false positive, closing the investigation.</summary>
    public void MarkFalsePositive()
    {
        EnsureStatusIsOneOf(SabotageIndicatorStatus.Open, SabotageIndicatorStatus.UnderInvestigation);
        Status = SabotageIndicatorStatus.FalsePositive;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISabotageIndicatorDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SabotageIndicatorStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new SabotageIndicatorDomainException(
            $"Invalid lifecycle transition from '{Status}' for SabotageIndicator {SabotageIndicatorId}.");
    }
}
