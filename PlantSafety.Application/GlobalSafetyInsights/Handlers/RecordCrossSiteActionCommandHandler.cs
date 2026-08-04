// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           RecordCrossSiteActionCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Handles RecordCrossSiteActionCommand -- records the action into the module-level
//                  audit ledger (IGlobalSafetyActionAuditRepository), NOT into a single
//                  GlobalSafetyInsightsAggregate instance. Rationale: CrossSiteActionEvent is not
//                  yet raised by any real command (the Multi-Site Coordination workflow that will
//                  raise it belongs to a later agent), so there is no natural "current" snapshot
//                  aggregate to attach the action to at this point in the build -- a dedicated,
//                  append-only audit ledger is the honest, working shell requested by the Phase 8
//                  §4 spec ("at minimum logs/records the action into the aggregate's audit trail").
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Repositories;

namespace PlantSafety.Application.GlobalSafetyInsights.Handlers;

/// <summary>Handles <see cref="RecordCrossSiteActionCommand"/>.</summary>
public sealed class RecordCrossSiteActionCommandHandler : IRequestHandler<RecordCrossSiteActionCommand, Result>
{
    private readonly IGlobalSafetyActionAuditRepository _auditRepository;
    private readonly ILogger<RecordCrossSiteActionCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public RecordCrossSiteActionCommandHandler(IGlobalSafetyActionAuditRepository auditRepository, ILogger<RecordCrossSiteActionCommandHandler> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> Handle(RecordCrossSiteActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _auditRepository.RecordAsync(
                request.TenantId, request.AffectedSiteIds, request.ActionType, request.Description,
                request.InitiatedByUserId, DateTime.UtcNow, request.RegionCode, request.WorkflowInstanceId, cancellationToken,
                request.CorrelationId, request.CausationId);

            _logger.LogInformation(
                "RecordCrossSiteAction: recorded '{ActionType}' affecting {SiteCount} site(s) for Tenant {TenantId}.",
                request.ActionType, request.AffectedSiteIds.Count, request.TenantId);

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record CrossSiteAction for Tenant {TenantId}", request.TenantId);

            return Result.Failure(new ApplicationError(
                "GlobalSafetyInsights.CrossSiteActionRecordingFailed",
                "Unable to record the cross-site action at this time.",
                Guid.NewGuid()));
        }
    }
}
