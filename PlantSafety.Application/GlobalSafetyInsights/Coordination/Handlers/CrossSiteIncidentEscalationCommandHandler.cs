// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteIncidentEscalationCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Handles CrossSiteIncidentEscalationCommand. For each affected site: reads the
//                  site's most recent non-terminal Incident (Reported/Assigned/Escalated -- via the
//                  real IIncidentReadRepository) and dispatches the real EscalateIncidentCommand
//                  against it, or -- when no open Incident exists at that site -- dispatches the
//                  real CreateIncidentCommand to create a new cross-site-flagged Incident. Both
//                  dispatches go through ISender only (Handbook Part IV §4.2.2 Zero Cross-Module
//                  Leakage). Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Escalate;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Handlers;

/// <summary>Handles <see cref="CrossSiteIncidentEscalationCommand"/>.</summary>
public sealed class CrossSiteIncidentEscalationCommandHandler
    : IRequestHandler<CrossSiteIncidentEscalationCommand, Result<CrossSiteIncidentEscalationResult>>
{
    /// <summary>Well-known system-actor UserId used when no human initiated the coordinated action.</summary>
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-0000005a4553");

    /// <summary>Incident statuses considered "open" (eligible for escalation rather than requiring a new record).</summary>
    private static readonly HashSet<string> OpenStatuses = new(StringComparer.OrdinalIgnoreCase) { "Reported", "Assigned", "Escalated" };

    private readonly ISender _sender;
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly ILogger<CrossSiteIncidentEscalationCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public CrossSiteIncidentEscalationCommandHandler(
        ISender sender,
        IIncidentReadRepository incidentReadRepository,
        ILogger<CrossSiteIncidentEscalationCommandHandler> logger)
    {
        _sender = sender;
        _incidentReadRepository = incidentReadRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrossSiteIncidentEscalationResult>> Handle(CrossSiteIncidentEscalationCommand request, CancellationToken cancellationToken)
    {
        var actorId = request.InitiatedByUserId ?? SystemUserId;
        var outcomes = new List<CrossSiteIncidentSiteOutcome>();

        foreach (var siteId in request.AffectedSiteIds)
        {
            try
            {
                var (items, _) = await _incidentReadRepository.ListAsync(
                    tenantId: request.TenantId,
                    siteId: siteId,
                    status: null,
                    severity: null,
                    occurredFromUtc: null,
                    occurredToUtc: null,
                    orderBy: "IncidentOccurredAtUtc",
                    sortDirection: "DESC",
                    pageNumber: 1,
                    pageSize: 25,
                    cancellationToken: cancellationToken);

                var openIncident = items.FirstOrDefault(i => OpenStatuses.Contains(i.Status));

                if (openIncident is not null)
                {
                    var incidentId = await _sender.Send(new EscalateIncidentCommand
                    {
                        CommandId = Guid.NewGuid(),
                        CorrelationId = request.CorrelationId,
                        CausationId = request.CausationId,
                        RequestedAtUtc = DateTime.UtcNow,
                        ApiVersionToken = "v1",
                        TenantId = request.TenantId,
                        ActorId = actorId,
                        IncidentId = openIncident.IncidentId,
                        Reason = request.Reason,
                    }, cancellationToken);

                    outcomes.Add(new CrossSiteIncidentSiteOutcome(siteId, true, incidentId, "Escalated", null));
                }
                else
                {
                    var nowUtc = DateTimeOffset.UtcNow;
                    var incidentId = await _sender.Send(new CreateIncidentCommand
                    {
                        CommandId = Guid.NewGuid(),
                        CorrelationId = request.CorrelationId,
                        CausationId = request.CausationId,
                        RequestedAtUtc = DateTime.UtcNow,
                        ApiVersionToken = "v1",
                        TenantId = request.TenantId,
                        ActorId = actorId,
                        SiteId = siteId,
                        Title = "[Cross-Site Escalation] " + request.Reason,
                        Description = $"Auto-created by the Multi-Site Coordination workflow: {request.Reason}",
                        IncidentOccurredAtUtc = nowUtc,
                        Severity = "Critical",
                        Location = "Cross-Site Coordination",
                        ReportedByWorkerId = actorId,
                        AssignedInvestigatorId = null,
                    }, cancellationToken);

                    outcomes.Add(new CrossSiteIncidentSiteOutcome(siteId, true, incidentId, "Created", null));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CrossSiteIncidentEscalation: failed for Site {SiteId}, Tenant {TenantId}", siteId, request.TenantId);
                outcomes.Add(new CrossSiteIncidentSiteOutcome(siteId, false, null, "Failed", "Unable to escalate or create the incident for this site at this time."));
            }
        }

        var result = new CrossSiteIncidentEscalationResult(outcomes);

        if (!result.AllSucceeded && outcomes.All(o => !o.Success))
        {
            return Result<CrossSiteIncidentEscalationResult>.Failure(new ApplicationError(
                "GlobalSafetyInsights.Coordination.IncidentEscalationFailed",
                "Unable to escalate incidents at any affected site.",
                request.CorrelationId));
        }

        return Result<CrossSiteIncidentEscalationResult>.Success(result);
    }
}
