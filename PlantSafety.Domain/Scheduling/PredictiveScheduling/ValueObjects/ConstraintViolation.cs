// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ConstraintViolation.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object describing a single constraint rule violation
//                  raised while evaluating a candidate ScheduleAssignment.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

/// <summary>A single constraint rule violation raised against a candidate schedule assignment.</summary>
public sealed record ConstraintViolation
{
    /// <summary>Stable machine-readable name of the rule that raised the violation (e.g. "MachineAvailability").</summary>
    public required string RuleName { get; init; }

    /// <summary>Severity of the violation.</summary>
    public required ConstraintSeverity Severity { get; init; }

    /// <summary>Human-readable explanation of the violation.</summary>
    public required string Message { get; init; }

    /// <summary>Identity of the task the violation pertains to.</summary>
    public required SchedulableTaskId TaskId { get; init; }

    /// <summary>Creates a Blocking-severity violation.</summary>
    public static ConstraintViolation Blocking(string ruleName, SchedulableTaskId taskId, string message) =>
        new() { RuleName = ruleName, Severity = ConstraintSeverity.Blocking, Message = message, TaskId = taskId };

    /// <summary>Creates a Warning-severity violation.</summary>
    public static ConstraintViolation Warn(string ruleName, SchedulableTaskId taskId, string message) =>
        new() { RuleName = ruleName, Severity = ConstraintSeverity.Warning, Message = message, TaskId = taskId };
}
