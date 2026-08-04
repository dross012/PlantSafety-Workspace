// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermit.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a permitted water discharge authorization issued to the
//                  facility (Clean Water Act / NPDES-style permit). Models issuance, renewal,
//                  pre-expiration flagging, suspension, and revocation. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates;

/// <summary>Aggregate root for a permitted water discharge authorization.</summary>
public sealed partial class DischargePermit
{
    // ---- Identity ----------------------------------------------------------
    /// <summary>Strong-typed identity of this DischargePermit aggregate.</summary>
    public DischargePermitId DischargePermitId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DischargePermitStatus Status { get; private set; }

    // ---- Permit details ------------------------------------------------------
    /// <summary>Regulator-assigned permit number.</summary>
    public string PermitNumber { get; private set; } = string.Empty;

    /// <summary>Regulatory program under which this permit was issued.</summary>
    public RegulatoryProgramType RegulatoryProgram { get; private set; }

    /// <summary>Name of the issuing regulatory authority.</summary>
    public string IssuingAuthority { get; private set; } = string.Empty;

    /// <summary>UTC instant the permit became effective.</summary>
    public UtcTimestamp EffectiveAtUtc { get; private set; }

    /// <summary>UTC instant the permit expires.</summary>
    public UtcTimestamp ExpiresAtUtc { get; private set; }

    /// <summary>Number of times this permit has been renewed.</summary>
    public int RenewalCount { get; private set; }

    /// <summary>Free-text summary of the permit conditions (NPDES condition mapping reference).</summary>
    public string PermitConditionSummary { get; private set; } = string.Empty;

    /// <summary>Reason recorded for the most recent suspension or revocation, if any.</summary>
    public string? LifecycleActionReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------
    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------
    private readonly List<IDischargePermitDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDischargePermitDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDischargePermitDomainEvent>(_domainEvents);

    // For rehydration only.
    private DischargePermit() { }

    // ---- Factory: Create (Issue) -----------------------------------------------------------
    /// <summary>Issues a new discharge permit and raises <see cref="DischargePermitIssuedEvent"/>.</summary>
    public static DischargePermit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitNumber = "",
        RegulatoryProgramType regulatoryProgram = RegulatoryProgramType.Npdes,
        string issuingAuthority = "",
        UtcTimestamp effectiveAtUtc = default,
        UtcTimestamp expiresAtUtc = default,
        string permitConditionSummary = "")
    {
        var agg = new DischargePermit
        {
            DischargePermitId = DischargePermitId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = DischargePermitStatus.Active,
            PermitNumber = (permitNumber ?? string.Empty).Trim(),
            RegulatoryProgram = regulatoryProgram,
            IssuingAuthority = (issuingAuthority ?? string.Empty).Trim(),
            EffectiveAtUtc = effectiveAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            RenewalCount = 0,
            PermitConditionSummary = (permitConditionSummary ?? string.Empty).Trim(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DischargePermitIssuedEvent(
            DischargePermitEventId.New(), agg.DischargePermitId, agg.PermitNumber, regulatoryProgram,
            effectiveAtUtc, expiresAtUtc, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Renew -----------------------------------------------
    /// <summary>Renews the permit through <paramref name="newExpiresAtUtc"/>.</summary>
    public void Renew(UtcTimestamp newExpiresAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePermitLifecycleRules.CanRenew(Status))
            throw new DischargePermitInvalidStatusTransitionException(DischargePermitId, Status.ToString(), DischargePermitStatus.Active.ToString(), correlationId, causationId);

        ExpiresAtUtc = newExpiresAtUtc;
        RenewalCount++;
        Status = DischargePermitStatus.Active;

        AddDomainEvent(new DischargePermitRenewedEvent(
            DischargePermitEventId.New(), DischargePermitId, newExpiresAtUtc, RenewalCount, newExpiresAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: FlagExpiringSoon -----------------------------------------------
    /// <summary>Flags the permit as approaching expiration (Scheduling pre-expiration reminder hook).</summary>
    public void FlagExpiringSoon(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePermitLifecycleRules.CanFlagExpiringSoon(Status))
            throw new DischargePermitInvalidStatusTransitionException(DischargePermitId, Status.ToString(), DischargePermitStatus.ExpiringSoon.ToString(), correlationId, causationId);

        Status = DischargePermitStatus.ExpiringSoon;

        AddDomainEvent(new DischargePermitExpiringSoonEvent(
            DischargePermitEventId.New(), DischargePermitId, ExpiresAtUtc, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------------
    /// <summary>Suspends the permit.</summary>
    public void Suspend(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePermitLifecycleRules.CanSuspend(Status))
            throw new DischargePermitInvalidStatusTransitionException(DischargePermitId, Status.ToString(), DischargePermitStatus.Suspended.ToString(), correlationId, causationId);

        Status = DischargePermitStatus.Suspended;
        LifecycleActionReason = reason;

        AddDomainEvent(new DischargePermitSuspendedEvent(
            DischargePermitEventId.New(), DischargePermitId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Revoke -----------------------------------------------
    /// <summary>Revokes the permit.</summary>
    public void Revoke(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePermitLifecycleRules.CanRevoke(Status))
            throw new DischargePermitInvalidStatusTransitionException(DischargePermitId, Status.ToString(), DischargePermitStatus.Revoked.ToString(), correlationId, causationId);

        Status = DischargePermitStatus.Revoked;
        LifecycleActionReason = reason;

        AddDomainEvent(new DischargePermitRevokedEvent(
            DischargePermitEventId.New(), DischargePermitId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------
    private void AddDomainEvent(IDischargePermitDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}