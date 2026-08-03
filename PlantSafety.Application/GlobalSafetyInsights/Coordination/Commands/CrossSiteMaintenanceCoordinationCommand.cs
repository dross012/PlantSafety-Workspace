// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteMaintenanceCoordinationCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Dispatches a real maintenance-response WorkOrder to each affected site via the
//                  real Operational.WorkOrders CreateWorkOrderCommand. (Scheduling.Rollup exposes
//                  only read-side rollup queries -- SchedulingRollupQueries/QueryHandlers -- no
//                  write command surface exists there to dispatch to; WorkOrder creation is the
//                  real, dispatchable maintenance-coordination action, and the rollup views will
//                  naturally reflect the newly created WorkOrders on their next read.) Dispatched
//                  by MultiSiteActionRouter only via ISender (Handbook Part IV §4.2.2 Zero
//                  Cross-Module Leakage). Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

/// <summary>Coordinates a maintenance response (WorkOrder creation) across a set of affected sites, as part of a governed cross-site coordinated action.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="AffectedSiteIds">Sites the maintenance coordination must be routed to (already policy-eligible; see MultiSiteActionRouter).</param>
/// <param name="MachineId">Identifier of the machine/equipment family targeted by this coordinated maintenance response.</param>
/// <param name="MachineCode">Human-readable machine code (format M-###).</param>
/// <param name="Priority">Priority classification (Low, Medium, High, Critical).</param>
/// <param name="Description">Description of the work to be performed at each affected site.</param>
/// <param name="DueAtUtc">UTC instant the work order is due at every affected site.</param>
/// <param name="InitiatedByUserId">User that initiated the coordinated action, if human-triggered; <see langword="null"/> for a system-triggered (auto-escalated) action.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering anomaly/request.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering anomaly/request.</param>
public sealed record CrossSiteMaintenanceCoordinationCommand(
    Guid TenantId,
    IReadOnlyList<Guid> AffectedSiteIds,
    Guid MachineId,
    string MachineCode,
    string Priority,
    string Description,
    DateTime DueAtUtc,
    Guid? InitiatedByUserId,
    Guid CorrelationId,
    Guid CausationId) : IRequest<Result<CrossSiteMaintenanceCoordinationResult>>;

/// <summary>Per-site outcome of a <see cref="CrossSiteMaintenanceCoordinationCommand"/>.</summary>
/// <param name="SiteId">The site this outcome pertains to.</param>
/// <param name="Success">Whether the dispatch to Operational.WorkOrders succeeded for this site.</param>
/// <param name="WorkOrderId">The newly created WorkOrder's identity, when <see cref="Success"/> is <see langword="true"/>.</param>
/// <param name="ErrorMessage">Sanitized failure reason, when <see cref="Success"/> is <see langword="false"/>.</param>
public sealed record CrossSiteMaintenanceCoordinationSiteOutcome(Guid SiteId, bool Success, Guid? WorkOrderId, string? ErrorMessage);

/// <summary>Result of a <see cref="CrossSiteMaintenanceCoordinationCommand"/> execution.</summary>
/// <param name="Outcomes">One outcome per entry in <see cref="CrossSiteMaintenanceCoordinationCommand.AffectedSiteIds"/>.</param>
public sealed record CrossSiteMaintenanceCoordinationResult(IReadOnlyList<CrossSiteMaintenanceCoordinationSiteOutcome> Outcomes)
{
    /// <summary><see langword="true"/> when every affected site's WorkOrder was successfully created.</summary>
    public bool AllSucceeded => Outcomes.All(o => o.Success);
}
