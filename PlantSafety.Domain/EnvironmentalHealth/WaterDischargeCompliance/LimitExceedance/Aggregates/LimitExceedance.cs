// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedance.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a recorded exceedance of a DischargeLimit and its response
//                  workflow. Severity feeds RiskManagement; repeat exceedances (tracked via
//                  RepeatExceedanceCount) trigger elevated inspection frequency per
//                  LimitExceedanceLifecycleRules.RequiresElevatedInspectionFrequency.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates;

/// <summary>Aggregate root for a recorded discharge limit exceedance and its response workflow.</summary>
public sealed partial class LimitExceedance
{
    /// <summary>Strong-typed identity of this LimitExceedance aggregate.</summary>
    public LimitExceedanceId LimitExceedanceId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public LimitExceedanceStatus Status { get; private set; }

    /// <summary>Identity of the DischargePoint where the exceedance occurred.</summary>
    public Guid DischargePointId { get; private set; }

    /// <summary>Identity of the EffluentSample that triggered this exceedance.</summary>
    public Guid EffluentSampleId { get; private set; }

    /// <summary>Identity of the DischargeLimit that was exceeded.</summary>
    public Guid DischargeLimitId { get; private set; }

    /// <summary>Display name of the exceeded parameter.</summary>
    public string ParameterName { get; private set; } = string.Empty;

    /// <summary>Measured value that triggered the exceedance.</summary>
    public decimal MeasuredValue { get; private set; }

    /// <summary>Limit value that was exceeded.</summary>
    public decimal LimitValue { get; private set; }

    /// <summary>Percentage by which the limit was exceeded.</summary>
    public decimal ExceedancePercentage { get; private set; }

    /// <summary>Severity classification (feeds RiskManagement risk hook).</summary>
    public ExceedanceSeverity Severity { get; private set; }

    /// <summary>UTC instant the exceedance was detected.</summary>
    public UtcTimestamp DetectedAtUtc { get; private set; }

    /// <summary>Count of exceedances for the same parameter/point in the trailing evaluation window.
    /// Used by <c>LimitExceedanceLifecycleRules.RequiresElevatedInspectionFrequency</c>.</summary>
    public int RepeatExceedanceCount { get; private set; }

    /// <summary>UTC instant the response workflow was initiated, if any.</summary>
    public UtcTimestamp? ResponseInitiatedAtUtc { get; private set; }

    /// <summary>Identity of the user who initiated the response, if any.</summary>
    public UserId? ResponseInitiatedByUserId { get; private set; }

    /// <summary>Notes recorded when the response was initiated.</summary>
    public string? ResponseNotes { get; private set; }

    /// <summary>UTC instant the response was closed, if any.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>Identity of the user who closed the response, if any.</summary>
    public UserId? ClosedByUserId { get; private set; }

    /// <summary>Resolution notes recorded when the response was closed.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    private readonly List<ILimitExceedanceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ILimitExceedanceDomainEvent> DomainEvents
        => new ReadOnlyCollection<ILimitExceedanceDomainEvent>(_domainEvents);

    private LimitExceedance() { }

    /// <summary>Records a new limit exceedance and raises <see cref="DischargeLimitExceededEvent"/>.</summary>
    public static LimitExceedance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid dischargePointId = default,
        Guid effluentSampleId = default,
        Guid dischargeLimitId = default,
        string parameterName = "",
        decimal measuredValue = 0m,
        decimal limitValue = 0m,
        ExceedanceSeverity severity = ExceedanceSeverity.Minor,
        UtcTimestamp detectedAtUtc = default,
        int repeatExceedanceCount = 0)
    {
        var exceedancePercentage = limitValue == 0m ? 0m : Math.Round((measuredValue - limitValue) / limitValue * 100m, 2);

        var agg = new LimitExceedance
        {
            LimitExceedanceId = LimitExceedanceId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = LimitExceedanceStatus.Detected,
            DischargePointId = dischargePointId,
            EffluentSampleId = effluentSampleId,
            DischargeLimitId = dischargeLimitId,
            ParameterName = (parameterName ?? string.Empty).Trim(),
            MeasuredValue = measuredValue,
            LimitValue = limitValue,
            ExceedancePercentage = exceedancePercentage,
            Severity = severity,
            DetectedAtUtc = detectedAtUtc,
            RepeatExceedanceCount = repeatExceedanceCount,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DischargeLimitExceededEvent(
            LimitExceedanceEventId.New(), agg.LimitExceedanceId, dischargePointId, agg.ParameterName,
            measuredValue, limitValue, exceedancePercentage, severity, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Initiates the exceedance response workflow.</summary>
    public void InitiateResponse(UserId initiatedByUserId, string? notes, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!LimitExceedanceLifecycleRules.CanInitiateResponse(Status))
            throw new LimitExceedanceInvalidStatusTransitionException(LimitExceedanceId, Status.ToString(), LimitExceedanceStatus.ResponseInitiated.ToString(), correlationId, causationId);

        Status = LimitExceedanceStatus.ResponseInitiated;
        ResponseInitiatedAtUtc = occurredAtUtc;
        ResponseInitiatedByUserId = initiatedByUserId;
        ResponseNotes = notes;

        AddDomainEvent(new ExceedanceResponseInitiatedEvent(
            LimitExceedanceEventId.New(), LimitExceedanceId, initiatedByUserId.Value, notes, occurredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Closes the exceedance response workflow.</summary>
    public void CloseResponse(UserId closedByUserId, string? resolutionNotes, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!LimitExceedanceLifecycleRules.CanCloseResponse(Status))
            throw new LimitExceedanceInvalidStatusTransitionException(LimitExceedanceId, Status.ToString(), LimitExceedanceStatus.Closed.ToString(), correlationId, causationId);

        Status = LimitExceedanceStatus.Closed;
        ClosedAtUtc = occurredAtUtc;
        ClosedByUserId = closedByUserId;
        ResolutionNotes = resolutionNotes;

        AddDomainEvent(new ExceedanceResponseClosedEvent(
            LimitExceedanceEventId.New(), LimitExceedanceId, closedByUserId.Value, resolutionNotes, occurredAtUtc, actor, correlationId, causationId));
    }

    private void AddDomainEvent(ILimitExceedanceDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}