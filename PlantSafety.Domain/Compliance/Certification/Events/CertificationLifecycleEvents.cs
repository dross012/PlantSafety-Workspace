// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Certification aggregate's full lifecycle
//                  (Verify/Renew/Expire/Close/Archive), raised alongside the pre-existing
//                  CertificationCreatedEvent. Phase 4 full build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Events;

/// <summary>Raised when a certification record is verified and activated (Created -&gt; Active).</summary>
public sealed record CertificationVerifiedEvent(
    CertificationEventId EventId,
    CertificationId CertificationId,
    UserId VerifiedBy,
    UtcTimestamp VerifiedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICertificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CertificationVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => VerifiedAtUtc;
}

/// <summary>Raised when an active certification is renewed, extending its expiry (status unchanged).</summary>
public sealed record CertificationRenewedEvent(
    CertificationEventId EventId,
    CertificationId CertificationId,
    UserId RenewedBy,
    UtcTimestamp NewExpiresAtUtc,
    UtcTimestamp RenewedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICertificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CertificationRenewed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RenewedAtUtc;
}

/// <summary>Raised when an active certification passes its validity window without renewal (Active -&gt; Completed).</summary>
public sealed record CertificationExpiredEvent(
    CertificationEventId EventId,
    CertificationId CertificationId,
    UtcTimestamp ExpiredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICertificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CertificationExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ExpiredAtUtc;
}

/// <summary>Raised when a completed/expired certification is formally closed (Completed -&gt; Closed).</summary>
public sealed record CertificationClosedEvent(
    CertificationEventId EventId,
    CertificationId CertificationId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICertificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CertificationClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed certification is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record CertificationArchivedEvent(
    CertificationEventId EventId,
    CertificationId CertificationId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICertificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CertificationArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
