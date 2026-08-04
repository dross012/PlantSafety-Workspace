// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the PublicSafetyAgency directory lifecycle:
//                  registration, contact management, verification, and deactivation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Events;

/// <summary>Raised when a new external public-safety agency is registered in the directory.</summary>
public sealed record PublicSafetyAgencyRegisteredEvent(
    PublicSafetyAgencyEventId EventId,
    PublicSafetyAgencyId PublicSafetyAgencyId,
    PublicSafetyAgencyType AgencyType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicSafetyAgencyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PublicSafetyAgencyRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a named contact/desk is added to an agency.</summary>
public sealed record AgencyContactAddedEvent(
    PublicSafetyAgencyEventId EventId,
    PublicSafetyAgencyId PublicSafetyAgencyId,
    AgencyContactId AgencyContactId,
    AgencyContactRole Role,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicSafetyAgencyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AgencyContactAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an agency's contact data is confirmed current via scheduled verification.</summary>
public sealed record AgencyVerificationRecordedEvent(
    PublicSafetyAgencyEventId EventId,
    PublicSafetyAgencyId PublicSafetyAgencyId,
    UtcTimestamp NextVerificationDueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicSafetyAgencyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AgencyVerificationRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an agency's scheduled contact re-verification becomes overdue.</summary>
public sealed record AgencyVerificationOverdueEvent(
    PublicSafetyAgencyEventId EventId,
    PublicSafetyAgencyId PublicSafetyAgencyId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicSafetyAgencyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AgencyVerificationOverdue";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an agency is deactivated and will no longer receive notifications.</summary>
public sealed record AgencyDeactivatedEvent(
    PublicSafetyAgencyEventId EventId,
    PublicSafetyAgencyId PublicSafetyAgencyId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicSafetyAgencyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AgencyDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
