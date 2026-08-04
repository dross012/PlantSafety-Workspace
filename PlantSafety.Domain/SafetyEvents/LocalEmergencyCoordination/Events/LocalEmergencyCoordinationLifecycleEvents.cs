// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationLifecycleEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the LocalEmergencyCoordination aggregate's coordination
//                  workflows (mutual aid, joint response planning, live incident liaison,
//                  post-incident debrief) and base lifecycle transitions (Close/Archive). Each
//                  event implements ILocalEmergencyCoordinationDomainEvent, which is also the
//                  designated event-based integration point for a future PublicSafetyCommunication
//                  module. Mirrors
//                  PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events.EmergencyManagementLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;

/// <summary>Raised when a mutual aid agreement is established with an external local emergency responder agency.</summary>
public sealed record MutualAidAgreementEstablishedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    MutualAidAgreementId MutualAidAgreementId,
    string AgencyName,
    UtcTimestamp EstablishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MutualAidAgreementEstablished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EstablishedAtUtc;
}

/// <summary>Raised when a joint response plan is activated for an incident.</summary>
public sealed record JointResponsePlanActivatedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    JointResponsePlanId JointResponsePlanId,
    Guid? IncidentId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "JointResponsePlanActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when a joint response plan is deactivated.</summary>
public sealed record JointResponsePlanDeactivatedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    JointResponsePlanId JointResponsePlanId,
    UtcTimestamp DeactivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "JointResponsePlanDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeactivatedAtUtc;
}

/// <summary>Raised when a user is assigned as the live-incident liaison to an external responder agency.</summary>
public sealed record ExternalResponderLiaisonAssignedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    ExternalResponderLiaisonId ExternalResponderLiaisonId,
    string AgencyName,
    UtcTimestamp AssignedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ExternalResponderLiaisonAssigned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AssignedAtUtc;
}

/// <summary>
/// Raised when a liaison communication is logged during a live incident. Unresolved liaison gaps
/// (no communication logged within the expected cadence) escalate through RiskManagement -- see
/// PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies.LiaisonGapEscalationPolicy.
/// </summary>
public sealed record LiaisonCommunicationLoggedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    ExternalResponderLiaisonId ExternalResponderLiaisonId,
    LiaisonLogEntryId LiaisonLogEntryId,
    UtcTimestamp LoggedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LiaisonCommunicationLogged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => LoggedAtUtc;
}

/// <summary>Raised when a post-incident debrief with external local emergency responder agencies is recorded.</summary>
public sealed record JointDebriefRecordedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    JointDebriefRecordId JointDebriefRecordId,
    Guid? IncidentId,
    UtcTimestamp RecordedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "JointDebriefRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}

/// <summary>Raised when a LocalEmergencyCoordination case is closed (Completed -&gt; Closed).</summary>
public sealed record LocalEmergencyCoordinationClosedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LocalEmergencyCoordinationClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed LocalEmergencyCoordination case is archived for EPCRA/LEPC compliance retention (Closed -&gt; Archived).</summary>
public sealed record LocalEmergencyCoordinationArchivedEvent(
    LocalEmergencyCoordinationEventId EventId,
    LocalEmergencyCoordinationId LocalEmergencyCoordinationId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILocalEmergencyCoordinationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LocalEmergencyCoordinationArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
