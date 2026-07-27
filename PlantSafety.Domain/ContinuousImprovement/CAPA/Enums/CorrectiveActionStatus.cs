// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionStatus.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for a CAPA (Corrective and Preventive Action) item tracked
//                  against a source safety event (Incident, NearMiss, AuditFinding, Observation).
//                  Values are taken verbatim from the workflow status vocabulary already
//                  documented on PlantSafety.Application.Features.ContinuousImprovement.Models.
//                  CAPA.CorrectiveActionDto.Status ("e.g., Open, InProgress, AwaitingVerification,
//                  Closed, Overdue") -- no new states were invented. This is a distinct type from
//                  PlantSafety.Domain.AccidentManagement.Enums.CorrectiveActionStatus (Open/
//                  InProgress/Completed/VerifiedEffective/Overdue), which models the unrelated
//                  Accident-child CorrectiveAction entity backed by a different table
//                  ([AccidentManagement].[CorrectiveAction]); per the Maximalist Module Rule
//                  ("zero cross-module leakage"), ContinuousImprovement.CAPA does not reference
//                  AccidentManagement's enum. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Enums;

/// <summary>Lifecycle states for a <see cref="Aggregates.CorrectiveAction"/> (CAPA) item.</summary>
public enum CorrectiveActionStatus
{
    /// <summary>Action has been created/assigned; work has not started. Initial state on Create.</summary>
    Open = 0,

    /// <summary>The assigned worker is actively working the action.</summary>
    InProgress = 1,

    /// <summary>Work is complete; evidence recorded, awaiting sign-off/verification.</summary>
    AwaitingVerification = 2,

    /// <summary>Action has been verified and formally closed. Terminal state.</summary>
    Closed = 3,

    /// <summary>Action is past its due date and not yet closed.</summary>
    Overdue = 4,
}
