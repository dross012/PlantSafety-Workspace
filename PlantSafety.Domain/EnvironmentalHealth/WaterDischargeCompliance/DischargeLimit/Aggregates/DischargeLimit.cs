// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimit.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a regulatory limit governing a parameter at a discharge
//                  point, mapped to a Clean Water Act / NPDES-style permit condition. Models
//                  definition, revision, and retirement. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates;

/// <summary>Aggregate root for a regulatory limit governing a parameter at a discharge point.</summary>
public sealed partial class DischargeLimit
{
    /// <summary>Strong-typed identity of this DischargeLimit aggregate.</summary>
    public DischargeLimitId DischargeLimitId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DischargeLimitStatus Status { get; private set; }

    /// <summary>Identity of the DischargePoint this limit governs.</summary>
    public Guid DischargePointId { get; private set; }

    /// <summary>Identity of the owning DischargePermit aggregate.</summary>
    public Guid DischargePermitId { get; private set; }

    /// <summary>Parameter this limit governs.</summary>
    public DischargeParameterType Parameter { get; private set; }

    /// <summary>Statistical basis over which this limit is evaluated.</summary>
    public LimitBasisType LimitBasis { get; private set; }

    /// <summary>Current limit value.</summary>
    public decimal LimitValue { get; private set; }

    /// <summary>Unit of measure for <see cref="LimitValue"/>.</summary>
    public string UnitOfMeasure { get; private set; } = string.Empty;

    /// <summary>UTC instant the limit became effective.</summary>
    public UtcTimestamp EffectiveAtUtc { get; private set; }

    /// <summary>Reference to the specific permit condition this limit maps to (NPDES condition mapping).</summary>
    public string PermitConditionReference { get; private set; } = string.Empty;

    /// <summary>Number of times this limit's value has been revised.</summary>
    public int RevisionCount { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    private readonly List<IDischargeLimitDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDischargeLimitDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDischargeLimitDomainEvent>(_domainEvents);

    private DischargeLimit() { }

    /// <summary>Defines a new discharge limit and raises <see cref="DischargeLimitDefinedEvent"/>.</summary>
    public static DischargeLimit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid dischargePointId = default,
        Guid dischargePermitId = default,
        DischargeParameterType parameter = DischargeParameterType.PH,
        LimitBasisType limitBasis = LimitBasisType.DailyMaximum,
        decimal limitValue = 0m,
        string unitOfMeasure = "",
        UtcTimestamp effectiveAtUtc = default,
        string permitConditionReference = "")
    {
        var agg = new DischargeLimit
        {
            DischargeLimitId = DischargeLimitId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = DischargeLimitStatus.Active,
            DischargePointId = dischargePointId,
            DischargePermitId = dischargePermitId,
            Parameter = parameter,
            LimitBasis = limitBasis,
            LimitValue = limitValue,
            UnitOfMeasure = (unitOfMeasure ?? string.Empty).Trim(),
            EffectiveAtUtc = effectiveAtUtc,
            PermitConditionReference = (permitConditionReference ?? string.Empty).Trim(),
            RevisionCount = 0,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DischargeLimitDefinedEvent(
            DischargeLimitEventId.New(), agg.DischargeLimitId, dischargePointId, parameter, limitValue, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Revises the limit value.</summary>
    public void Revise(decimal newLimitValue, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargeLimitLifecycleRules.CanRevise(Status))
            throw new DischargeLimitInvalidStatusTransitionException(DischargeLimitId, Status.ToString(), DischargeLimitStatus.Active.ToString(), correlationId, causationId);

        LimitValue = newLimitValue;
        RevisionCount++;

        AddDomainEvent(new DischargeLimitRevisedEvent(
            DischargeLimitEventId.New(), DischargeLimitId, newLimitValue, RevisionCount, occurredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Retires the limit.</summary>
    public void Retire(string? reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargeLimitLifecycleRules.CanRetire(Status))
            throw new DischargeLimitInvalidStatusTransitionException(DischargeLimitId, Status.ToString(), DischargeLimitStatus.Retired.ToString(), correlationId, causationId);

        Status = DischargeLimitStatus.Retired;

        AddDomainEvent(new DischargeLimitRetiredEvent(
            DischargeLimitEventId.New(), DischargeLimitId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    private void AddDomainEvent(IDischargeLimitDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}