// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for GetSlaMonitorRiskQuery. Computed-on-read: fetches active
//   incidents, computes each row's SLA clock, and returns only those classified AtRisk or
//   Breached, ordered by remaining time ascending (most urgent first).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Queries;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Services;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.Enums;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.Services;

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Handlers;

/// <summary>Handles <see cref="GetSlaMonitorRiskQuery"/>.</summary>
public sealed class GetSlaMonitorRiskQueryHandler : IQueryHandler<GetSlaMonitorRiskQuery, PaginatedResult<SlaRiskDto>>
{
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly SlaPolicyResolver _policyResolver;

    /// <summary>Initialises dependencies.</summary>
    public GetSlaMonitorRiskQueryHandler(IIncidentReadRepository incidentReadRepository, SlaPolicyResolver policyResolver)
    {
        _incidentReadRepository = incidentReadRepository;
        _policyResolver = policyResolver;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<SlaRiskDto>> Handle(GetSlaMonitorRiskQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();
        var nowUtc = DateTimeOffset.UtcNow;

        var policies = await _policyResolver.ResolveAsync(request.TenantId, cancellationToken);

        var (allIncidents, _) = await _incidentReadRepository.ListAsync(
            tenantId: request.TenantId,
            siteId: null,
            status: null,
            severity: null,
            occurredFromUtc: null,
            occurredToUtc: null,
            orderBy: "IncidentOccurredAtUtc",
            sortDirection: "ASC",
            pageNumber: 1,
            pageSize: 1000,
            cancellationToken: cancellationToken);

        var riskRows = allIncidents
            .Where(i => i.Status is not ("Resolved" or "Archived"))
            .Select(row =>
            {
                var policy = SlaPolicyResolver.Resolve(policies, row.Severity);
                var result = IncidentSlaClockCalculator.Compute(
                    incidentOccurredAtUtc: row.IncidentOccurredAtUtc,
                    isAssigned: row.AssignedInvestigatorId.HasValue,
                    isEscalated: row.Status == "Escalated",
                    isResolvedOrArchived: false,
                    resolvedAtUtc: row.ClosedAtUtc,
                    nowUtc: nowUtc,
                    policy: policy);

                return (row, result);
            })
            .Where(x => x.result.Status is SlaStatus.AtRisk or SlaStatus.Breached)
            .OrderBy(x => x.result.Remaining)
            .Select(x => new SlaRiskDto
            {
                IncidentId = x.row.IncidentId,
                Title = x.row.Title,
                Severity = x.row.Severity,
                SlaRemaining = x.result.Remaining,
                TechnicianName = x.row.AssignedInvestigatorId?.ToString(),
                RecommendedAction = x.result.RecommendedAction,
            })
            .ToList();

        var pagedItems = riskRows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<SlaRiskDto>(
            items: pagedItems,
            totalCount: riskRows.Count,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }
}
