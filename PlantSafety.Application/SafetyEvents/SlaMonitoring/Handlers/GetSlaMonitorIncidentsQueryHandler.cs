// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for GetSlaMonitorIncidentsQuery. Computed-on-read: fetches
//   active (non-Resolved, non-Archived) incidents via the existing IIncidentReadRepository and
//   applies IncidentSlaClockCalculator per row rather than persisting duplicate SLA state.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Queries;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Services;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.Services;

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Handlers;

/// <summary>Handles <see cref="GetSlaMonitorIncidentsQuery"/>.</summary>
public sealed class GetSlaMonitorIncidentsQueryHandler : IQueryHandler<GetSlaMonitorIncidentsQuery, PaginatedResult<SlaIncidentDto>>
{
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly SlaPolicyResolver _policyResolver;

    /// <summary>Initialises dependencies.</summary>
    public GetSlaMonitorIncidentsQueryHandler(IIncidentReadRepository incidentReadRepository, SlaPolicyResolver policyResolver)
    {
        _incidentReadRepository = incidentReadRepository;
        _policyResolver = policyResolver;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<SlaIncidentDto>> Handle(GetSlaMonitorIncidentsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();
        var nowUtc = DateTimeOffset.UtcNow;

        var policies = await _policyResolver.ResolveAsync(request.TenantId, cancellationToken);

        // SLA-active incidents are, by definition, not yet Resolved/Archived -- pull a generously
        // sized unpaged window and page in-memory after computing each row's SLA clock, since the
        // read-side stored procedures have no "SLA active" filter concept of their own.
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

        var activeRows = allIncidents.Where(i => i.Status is not ("Resolved" or "Archived")).ToList();

        var items = activeRows.Select(row =>
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

            return new SlaIncidentDto
            {
                IncidentId = row.IncidentId,
                Title = row.Title,
                Severity = row.Severity,
                SlaRemaining = result.Remaining,
                TechnicianName = row.AssignedInvestigatorId?.ToString(),
                Status = row.Status,
                EscalationState = row.Status == "Escalated" ? "Escalated" : "None",
            };
        }).ToList();

        var pagedItems = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<SlaIncidentDto>(
            items: pagedItems,
            totalCount: items.Count,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }
}
