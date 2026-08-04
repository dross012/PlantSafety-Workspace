// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the DischargePermit lifecycle: issuance, renewal,
//                  expiring-soon flagging, suspension, and revocation. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Events;

/// <summary>Raised when a new discharge permit is issued.</summary>
public sealed record DischargePermitIssuedEvent(
    DischargePermitEventId EventId,
    DischargePermitId DischargePermitId,
    string PermitNumber,
    RegulatoryProgramType RegulatoryProgram,
    UtcTimestamp EffectiveAtUtc,
    UtcTimestamp ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePermitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePermitIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a discharge permit is renewed.</summary>
public sealed record DischargePermitRenewedEvent(
    DischargePermitEventId EventId,
    DischargePermitId DischargePermitId,
    UtcTimestamp NewExpiresAtUtc,
    int RenewalCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePermitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePermitRenewed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a permit is flagged as expiring soon (Scheduling pre-expiration hook).</summary>
public sealed record DischargePermitExpiringSoonEvent(
    DischargePermitEventId EventId,
    DischargePermitId DischargePermitId,
    UtcTimestamp ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePermitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePermitExpiringSoon";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a permit is suspended by the issuing authority.</summary>
public sealed record DischargePermitSuspendedEvent(
    DischargePermitEventId EventId,
    DischargePermitId DischargePermitId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePermitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePermitSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a permit is revoked by the issuing authority.</summary>
public sealed record DischargePermitRevokedEvent(
    DischargePermitEventId EventId,
    DischargePermitId DischargePermitId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePermitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePermitRevoked";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}