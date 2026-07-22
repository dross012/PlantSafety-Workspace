// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           CorrectiveActionStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for a single CorrectiveAction (CAPA item) attached to an Accident.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AccidentManagement.Enums;

/// <summary>Lifecycle states for a single CorrectiveAction (CAPA item).</summary>
public enum CorrectiveActionStatus
{
    /// <summary>Action has been assigned but work has not started.</summary>
    Open = 0,

    /// <summary>Responsible person is actively working the action.</summary>
    InProgress = 1,

    /// <summary>Action work is complete; evidence submitted, awaiting effectiveness verification.</summary>
    Completed = 2,

    /// <summary>Action has been verified effective via follow-up inspection/testing (Full Risk Management Process, Step 7).</summary>
    VerifiedEffective = 3,

    /// <summary>Action is past its due date and not yet completed.</summary>
    Overdue = 4,
}
