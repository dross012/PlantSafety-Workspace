// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           CrossSiteActionLogEntryDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO projecting a single [GlobalSafetyInsights].[CrossSiteActionLog]
//                  entry -- backs the Global Safety Dashboard's corrective-action tracker and
//                  incident-timeline widgets. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.GlobalSafetyInsights.Models;

/// <summary>A single cross-site coordinated-action audit ledger entry.</summary>
public sealed record CrossSiteActionLogEntryDto
{
    /// <summary>Audit ledger entry identifier.</summary>
    public required Guid CrossSiteActionLogId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Every site affected by this coordinated action.</summary>
    public required IReadOnlyList<Guid> AffectedSiteIds { get; init; }

    /// <summary>Discriminator for the kind of coordinated action (e.g. "CorrectiveActionRollout", "IncidentEscalation").</summary>
    public required string ActionType { get; init; }

    /// <summary>Human-readable description of the action.</summary>
    public required string Description { get; init; }

    /// <summary>Identity of the user who initiated the action, if any.</summary>
    public Guid? InitiatedByUserId { get; init; }

    /// <summary>UTC instant the action was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>The region the action was resolved/routed in, if known.</summary>
    public string? RegionCode { get; init; }

    /// <summary>
    /// The Workflow.WorkflowInstance started to gate this action behind approval, if the coordinated
    /// action required one (see MultiSiteActionRouter.TryStartApprovalWorkflowAsync); <see langword="null"/>
    /// when the action executed immediately without an approval chain.
    /// </summary>
    public Guid? WorkflowInstanceId { get; init; }

    /// <summary>
    /// Correlation identifier propagated from the triggering CrossSiteActionEvent (Phase 9 §4
    /// observability-audit fix: previously computed end-to-end by MultiSiteActionRouter/
    /// RecordCrossSiteActionCommand but silently dropped before persistence, breaking queryable
    /// trace-back from this audit ledger to the originating request/mesh event). <see langword="null"/>
    /// for rows recorded before this column existed.
    /// </summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>
    /// Causation identifier propagated from the triggering CrossSiteActionEvent (see
    /// <see cref="CorrelationId"/> remarks). <see langword="null"/> for rows recorded before this
    /// column existed.
    /// </summary>
    public Guid? CausationId { get; init; }
}
