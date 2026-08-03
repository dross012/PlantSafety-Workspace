// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteMaintenanceCoordinationCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Handles CrossSiteMaintenanceCoordinationCommand. For each affected site,
//                  dispatches the real Operational.WorkOrders CreateWorkOrderCommand via ISender
//                  only (Handbook Part IV §4.2.2 Zero Cross-Module Leakage). Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;
using PlantSafety.Application.Operational.WorkOrders.Commands;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Handlers;

/// <summary>Handles <see cref="CrossSiteMaintenanceCoordinationCommand"/>.</summary>
public sealed class CrossSiteMaintenanceCoordinationCommandHandler
    : IRequestHandler<CrossSiteMaintenanceCoordinationCommand, Result<CrossSiteMaintenanceCoordinationResult>>
{
    /// <summary>Well-known system-actor UserId used when no human initiated the coordinated action.</summary>
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-0000005a4553");

    private readonly ISender _sender;
    private readonly ILogger<CrossSiteMaintenanceCoordinationCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public CrossSiteMaintenanceCoordinationCommandHandler(ISender sender, ILogger<CrossSiteMaintenanceCoordinationCommandHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrossSiteMaintenanceCoordinationResult>> Handle(CrossSiteMaintenanceCoordinationCommand request, CancellationToken cancellationToken)
    {
        var actorId = request.InitiatedByUserId ?? SystemUserId;
        var outcomes = new List<CrossSiteMaintenanceCoordinationSiteOutcome>();

        foreach (var siteId in request.AffectedSiteIds)
        {
            try
            {
                var result = await _sender.Send(new CreateWorkOrderCommand(
                    CommandId: Guid.NewGuid(),
                    CorrelationId: request.CorrelationId,
                    CausationId: request.CausationId,
                    RequestedAtUtc: DateTime.UtcNow,
                    ApiVersionToken: "v1",
                    TenantId: request.TenantId,
                    SiteId: siteId,
                    WorkOrderCode: $"WO-XS-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                    MachineId: request.MachineId,
                    MachineCode: request.MachineCode,
                    Priority: request.Priority,
                    Description: request.Description,
                    DueAtUtc: request.DueAtUtc,
                    RequestedByUserId: actorId), cancellationToken);

                if (result.IsSuccess)
                {
                    outcomes.Add(new CrossSiteMaintenanceCoordinationSiteOutcome(siteId, true, result.Value.WorkOrderId, null));
                }
                else
                {
                    _logger.LogWarning(
                        "CrossSiteMaintenanceCoordination: WorkOrder creation failed for Site {SiteId}, Tenant {TenantId}: [{ErrorCode}] {Message}",
                        siteId, request.TenantId, result.Error.ErrorCode, result.Error.Message);
                    outcomes.Add(new CrossSiteMaintenanceCoordinationSiteOutcome(siteId, false, null, result.Error.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CrossSiteMaintenanceCoordination: failed for Site {SiteId}, Tenant {TenantId}", siteId, request.TenantId);
                outcomes.Add(new CrossSiteMaintenanceCoordinationSiteOutcome(siteId, false, null, "Unable to create the work order for this site at this time."));
            }
        }

        var coordinationResult = new CrossSiteMaintenanceCoordinationResult(outcomes);

        if (outcomes.Count > 0 && outcomes.All(o => !o.Success))
        {
            return Result<CrossSiteMaintenanceCoordinationResult>.Failure(new ApplicationError(
                "GlobalSafetyInsights.Coordination.MaintenanceCoordinationFailed",
                "Unable to create work orders at any affected site.",
                request.CorrelationId));
        }

        return Result<CrossSiteMaintenanceCoordinationResult>.Success(coordinationResult);
    }
}
