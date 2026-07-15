// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Routing module (Volume R1) —
//   Dashboard, Detail, Steps, Version History, Diff Viewer, Simulation
//   Workspace, and Approval Workflow pages. Display-only — all diff/
//   simulation computation is server-side.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Routing.Models;

/// <summary>Single row in the Routing Dashboard list.</summary>
public sealed class RoutingSummaryDto
{
    /// <summary>Gets the routing identifier (UUIDv7).</summary>
    public Guid RoutingId { get; init; }

    /// <summary>Gets the routing display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the plant/site name.</summary>
    public string Plant { get; init; } = string.Empty;

    /// <summary>Gets the routing lifecycle status (Draft, Submitted, Review, Approved, Rejected).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current version label.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp this routing was last updated.</summary>
    public DateTimeOffset LastUpdatedUtc { get; init; }

    /// <summary>Gets the owner display name.</summary>
    public string Owner { get; init; } = string.Empty;
}

/// <summary>Full routing detail header (metadata panel).</summary>
public sealed class RoutingDetailDto
{
    /// <summary>Gets the routing identifier.</summary>
    public Guid RoutingId { get; init; }

    /// <summary>Gets the routing display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the plant/site name.</summary>
    public string Plant { get; init; } = string.Empty;

    /// <summary>Gets the routing lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current version label.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the current version identifier.</summary>
    public Guid VersionId { get; init; }

    /// <summary>Gets the effective start date (UTC).</summary>
    public DateTimeOffset EffectiveFromUtc { get; init; }

    /// <summary>Gets the effective end date (UTC), if any.</summary>
    public DateTimeOffset? EffectiveToUtc { get; init; }

    /// <summary>Gets the owner display name.</summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the last audit event.</summary>
    public DateTimeOffset LastAuditedUtc { get; init; }
}

/// <summary>Single routing step row.</summary>
public sealed class RoutingStepDto
{
    /// <summary>Gets the strictly-sequential step number (1..N, no gaps).</summary>
    public int StepNumber { get; init; }

    /// <summary>Gets the step name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the step status (Valid, Warning, Blocked).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the step instructions.</summary>
    public string Instructions { get; init; } = string.Empty;

    /// <summary>Gets the assigned resource names for this step.</summary>
    public IReadOnlyList<string> Resources { get; init; } = [];
}

/// <summary>Single version history timeline node.</summary>
public sealed class RoutingVersionDto
{
    /// <summary>Gets the version identifier.</summary>
    public Guid VersionId { get; init; }

    /// <summary>Gets the version label (e.g. "v3").</summary>
    public string VersionLabel { get; init; } = string.Empty;

    /// <summary>Gets the effective date (UTC).</summary>
    public DateTimeOffset EffectiveDateUtc { get; init; }

    /// <summary>Gets the version status (Approved, Pending, Rejected).</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Single pre-aligned diff row returned by the server (no client-side diff computation).</summary>
public sealed class RoutingDiffRowDto
{
    /// <summary>Gets the diff group (Steps, Materials, Resources).</summary>
    public string DiffGroup { get; init; } = string.Empty;

    /// <summary>Gets the field name that changed.</summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>Gets the change type (Addition, Deletion, Modification).</summary>
    public string ChangeType { get; init; } = string.Empty;

    /// <summary>Gets the previous version's value.</summary>
    public string? PreviousValue { get; init; }

    /// <summary>Gets the current version's value.</summary>
    public string? CurrentValue { get; init; }
}

/// <summary>Server-computed simulation result summary.</summary>
public sealed class RoutingSimulationResultDto
{
    /// <summary>Gets the total simulated duration in minutes.</summary>
    public int TotalDurationMinutes { get; init; }

    /// <summary>Gets the critical path step numbers, in order.</summary>
    public IReadOnlyList<int> CriticalPath { get; init; } = [];

    /// <summary>Gets the count of detected resource conflicts.</summary>
    public int ResourceConflictCount { get; init; }

    /// <summary>Gets the count of detected material shortages.</summary>
    public int MaterialShortageCount { get; init; }

    /// <summary>Gets the bottleneck step numbers flagged by the simulation.</summary>
    public IReadOnlyList<int> BottleneckSteps { get; init; } = [];
}

/// <summary>Single approval workflow stage/approver row.</summary>
public sealed class RoutingApproverDto
{
    /// <summary>Gets the approver display name.</summary>
    public string ApproverName { get; init; } = string.Empty;

    /// <summary>Gets the approver's role.</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>Gets the approver's decision status (Pending, Approved, Rejected).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the decision, if made.</summary>
    public DateTimeOffset? DecidedAtUtc { get; init; }
}

/// <summary>Full approval workflow state for a routing.</summary>
public sealed class RoutingApprovalWorkflowDto
{
    /// <summary>Gets the current stage name (Draft, Submitted, Review, Approved, Rejected).</summary>
    public string CurrentStage { get; init; } = string.Empty;

    /// <summary>Gets the assigned approvers for this workflow.</summary>
    public IReadOnlyList<RoutingApproverDto> Approvers { get; init; } = [];

    /// <summary>Gets a value indicating whether the current caller may approve/reject.</summary>
    public bool CanCurrentUserAct { get; init; }
}
