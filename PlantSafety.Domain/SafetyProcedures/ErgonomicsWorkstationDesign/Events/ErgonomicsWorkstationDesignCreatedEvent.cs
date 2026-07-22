// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real ergonomics/workstation-design procedure
//                  lifecycle: drafting, submission, publication, assessment scheduling/recording, and
//                  retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Events;

/// <summary>Raised when a new ergonomics/workstation-design procedure is drafted.</summary>
public sealed record ErgonomicsProcedureDraftedEvent(
    ErgonomicsWorkstationDesignEventId EventId,
    ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId,
    WorkstationDesignCategory DesignCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsWorkstationDesignDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsProcedureDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is submitted for approval.</summary>
public sealed record ErgonomicsProcedureSubmittedForApprovalEvent(
    ErgonomicsWorkstationDesignEventId EventId,
    ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsWorkstationDesignDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsProcedureSubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is approved and published.</summary>
public sealed record ErgonomicsProcedurePublishedEvent(
    ErgonomicsWorkstationDesignEventId EventId,
    ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId,
    UserId ApprovedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsWorkstationDesignDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsProcedurePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a workstation assessment is scheduled.</summary>
public sealed record ErgonomicsWorkstationAssessmentScheduledEvent(
    ErgonomicsWorkstationDesignEventId EventId,
    ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsWorkstationDesignDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsWorkstationAssessmentScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when a workstation assessment outcome is recorded.</summary>
public sealed record ErgonomicsWorkstationAssessmentRecordedEvent(
    ErgonomicsWorkstationDesignEventId EventId,
    ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId,
    int RiskFactorCount,
    decimal ErgonomicsScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsWorkstationDesignDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsWorkstationAssessmentRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure document is retired.</summary>
public sealed record ErgonomicsProcedureRetiredEvent(
    ErgonomicsWorkstationDesignEventId EventId,
    ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsWorkstationDesignDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsProcedureRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
