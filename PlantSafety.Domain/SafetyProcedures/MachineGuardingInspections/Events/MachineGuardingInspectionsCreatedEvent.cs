// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real machine guarding inspection lifecycle:
//                  scheduling, pass/deficiency execution outcome, corrective action, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Events;

/// <summary>Raised when a new inspection is scheduled.</summary>
public sealed record MachineGuardingInspectionProcedureScheduledEvent(
    MachineGuardingInspectionsEventId EventId,
    MachineGuardingInspectionsId MachineGuardingInspectionsId,
    MachineGuardingInspectionCategory InspectionCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingInspectionsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionProcedureScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the inspection passes with no deficiencies.</summary>
public sealed record MachineGuardingInspectionProcedurePassedEvent(
    MachineGuardingInspectionsEventId EventId,
    MachineGuardingInspectionsId MachineGuardingInspectionsId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingInspectionsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionProcedurePassed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when deficiencies are logged during execution.</summary>
public sealed record MachineGuardingInspectionDeficiencyLoggedEvent(
    MachineGuardingInspectionsEventId EventId,
    MachineGuardingInspectionsId MachineGuardingInspectionsId,
    int DeficiencyCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingInspectionsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionDeficiencyLogged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when corrective action begins to remediate logged deficiencies.</summary>
public sealed record MachineGuardingInspectionCorrectiveActionStartedEvent(
    MachineGuardingInspectionsEventId EventId,
    MachineGuardingInspectionsId MachineGuardingInspectionsId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingInspectionsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionCorrectiveActionStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the inspection is closed out and the next cycle scheduled.</summary>
public sealed record MachineGuardingInspectionClosedEvent(
    MachineGuardingInspectionsEventId EventId,
    MachineGuardingInspectionsId MachineGuardingInspectionsId,
    UtcTimestamp NextDueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingInspectionsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
