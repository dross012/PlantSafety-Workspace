// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteCorrectiveActionCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Handles CrossSiteCorrectiveActionCommand. For each affected site, dispatches
//                  the real ContinuousImprovement.CAPA CreateCorrectiveActionCommand via ISender
//                  only (Handbook Part IV §4.2.2 Zero Cross-Module Leakage). Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Handlers;

/// <summary>Handles <see cref="CrossSiteCorrectiveActionCommand"/>.</summary>
public sealed class CrossSiteCorrectiveActionCommandHandler
    : IRequestHandler<CrossSiteCorrectiveActionCommand, Result<CrossSiteCorrectiveActionResult>>
{
    /// <summary>Well-known system-actor UserId used when no human initiated the coordinated action.</summary>
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-0000005a4553");

    private readonly ISender _sender;
    private readonly ILogger<CrossSiteCorrectiveActionCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public CrossSiteCorrectiveActionCommandHandler(ISender sender, ILogger<CrossSiteCorrectiveActionCommandHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrossSiteCorrectiveActionResult>> Handle(CrossSiteCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var actorId = request.InitiatedByUserId ?? SystemUserId;
        var outcomes = new List<CrossSiteCorrectiveActionSiteOutcome>();

        foreach (var siteId in request.AffectedSiteIds)
        {
            try
            {
                var correctiveActionId = await _sender.Send(new CreateCorrectiveActionCommand
                {
                    CommandId = Guid.NewGuid(),
                    CorrelationId = request.CorrelationId,
                    CausationId = request.CausationId,
                    RequestedAtUtc = DateTime.UtcNow,
                    ApiVersionToken = "v1",
                    TenantId = request.TenantId,
                    ActorId = actorId,
                    SiteId = siteId,
                    Title = request.Title,
                    Description = request.Description,
                    ActionType = "Corrective",
                    SourceEventId = request.SourceEventId,
                    SourceEventType = request.SourceEventType,
                    Priority = request.Priority,
                    AssignedToWorkerId = request.AssignedToWorkerId,
                    DueAtUtc = request.DueAtUtc,
                }, cancellationToken);

                outcomes.Add(new CrossSiteCorrectiveActionSiteOutcome(siteId, true, correctiveActionId, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CrossSiteCorrectiveAction: failed for Site {SiteId}, Tenant {TenantId}", siteId, request.TenantId);
                outcomes.Add(new CrossSiteCorrectiveActionSiteOutcome(siteId, false, null, "Unable to create the corrective action for this site at this time."));
            }
        }

        var result = new CrossSiteCorrectiveActionResult(outcomes);

        if (outcomes.Count > 0 && outcomes.All(o => !o.Success))
        {
            return Result<CrossSiteCorrectiveActionResult>.Failure(new ApplicationError(
                "GlobalSafetyInsights.Coordination.CorrectiveActionRolloutFailed",
                "Unable to roll out the corrective action to any affected site.",
                request.CorrelationId));
        }

        return Result<CrossSiteCorrectiveActionResult>.Success(result);
    }
}
