// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertLifecycleService.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical domain service responsible for lifecycle transitions of
//      Alerts. Applies lifecycle rules, enforces invariants, and emits
//      domain events. No infrastructure logic.
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Enums;
using PlantSafety.Domain.Operational.Alerts.Exceptions;
using PlantSafety.Domain.Operational.Alerts.Rules;
using PlantSafety.Domain.Operational.Alerts.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Operational.Alerts.Services;

public sealed class AlertLifecycleService
{
    public static void Acknowledge(
        Alert alert,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        AlertNotes? notes = null)
    {
        if (!AlertLifecycleRules.CanAcknowledge(alert.Status))
            throw new AlertInvalidStatusTransitionException(
                alert.Status, AlertStatus.Acknowledged, correlationId, causationId);

        alert.Acknowledge(timestamp, actor, correlationId, causationId, notes);
    }

    public static void StartProgress(
        Alert alert,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertLifecycleRules.CanStartProgress(alert.Status))
            throw new AlertInvalidStatusTransitionException(
                alert.Status, AlertStatus.InProgress, correlationId, causationId);

        alert.StartProgress(timestamp, actor, correlationId, causationId);
    }

    public static void Escalate(
        Alert alert,
        EscalationReason reason,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertLifecycleRules.CanEscalate(alert.Status))
            throw new AlertInvalidStatusTransitionException(
                alert.Status, AlertStatus.Escalated, correlationId, causationId);

        alert.Escalate(reason, timestamp, actor, correlationId, causationId);
    }

    public static void Resolve(
        Alert alert,
        ResolutionSummary summary,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertLifecycleRules.CanResolve(alert.Status))
            throw new AlertInvalidStatusTransitionException(
                alert.Status, AlertStatus.Resolved, correlationId, causationId);

        alert.Resolve(summary, timestamp, actor, correlationId, causationId);
    }

    public static void Close(
        Alert alert,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertLifecycleRules.CanClose(alert.Status))
            throw new AlertInvalidStatusTransitionException(
                alert.Status, AlertStatus.Closed, correlationId, causationId);

        alert.Close(timestamp, actor, correlationId, causationId);
    }

    public static void Archive(
        Alert alert,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertLifecycleRules.CanArchive(alert.Status))
            throw new AlertInvalidStatusTransitionException(
                alert.Status, AlertStatus.Archived, correlationId, causationId);

        alert.Archive(timestamp, actor, correlationId, causationId);
    }
}
