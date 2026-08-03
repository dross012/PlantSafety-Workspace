// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Audit
//  Layer:          SharedKernel
//  File:           DrDrillResult.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.4 (Disaster Recovery) domain event raised whenever a DR drill is
//                  logged as completed. No real DR drill infrastructure exists in this sandbox
//                  (no real secondary region/replica to actually fail over to -- see
//                  docs\Phase7\multi-region-dr-runbook.md); this event exists so that when a
//                  human/ops team DOES run a real drill against real infrastructure, the fact
//                  that it happened -- date, scope, outcome, issues found -- is durably
//                  recorded, letting the platform track its own DR-readiness posture over time.
//                  Modeled on the same pattern as SecretRotationAuditEvent (Phase 6 §6.3): lives
//                  in SharedKernel because it is a cross-cutting platform concern, not scoped to
//                  a single bounded context. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Base;

namespace PlantSafety.SharedKernel.Audit;

/// <summary>
/// Raised whenever a disaster-recovery drill is logged as completed. Carries the drill's date,
/// scope, outcome, and any issues found -- lets the platform track that drills actually happened
/// even though this codebase cannot itself execute a real multi-region failover drill.
/// </summary>
/// <param name="DrillId">Unique identifier of this drill record.</param>
/// <param name="DrillDateUtc">UTC date/time the drill was performed.</param>
/// <param name="Scope">
/// Human-readable description of what the drill exercised (e.g. "SQL Server Always On manual
/// failover, US East -> US West", "RabbitMQ cluster node loss simulation").
/// </param>
/// <param name="Outcome">The drill's outcome (e.g. "Passed", "Failed", "PartialSuccess").</param>
/// <param name="IssuesFound">Any issues identified during the drill (empty if none).</param>
public sealed record DrDrillResult(
    Guid DrillId,
    DateTime DrillDateUtc,
    string Scope,
    string Outcome,
    IReadOnlyList<string> IssuesFound) : DomainEvent
{
    /// <inheritdoc/>
    public override string EventType => "DisasterRecovery.DrillLogged";
}
