// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for GetSlaMonitorTrendQuery. Computed-on-read: fetches
//   incidents occurring within the trend window, computes each row's SLA clock (using
//   ResolvedAtUtc for resolved/archived rows so historical compliance is measured against the
//   actual resolution time, not "now"), and rolls up compliance %, breach count, and severity
//   distribution.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Queries;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Services;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.Enums;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.Services;

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Handlers;

/// <summary>Handles <see cref="GetSlaMonitorTrendQuery"/>.</summary>
public sealed class GetSlaMonitorTrendQueryHandler : IQueryHandler<GetSlaMonitorTrendQuery, SlaTrendDto>
{
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly SlaPolicyResolver _policyResolver;

    /// <summary>Initialises dependencies.</summary>
    public GetSlaMonitorTrendQueryHandler(IIncidentReadRepository incidentReadRepository, SlaPolicyResolver policyResolver)
    {
        _incidentReadRepository = incidentReadRepository;
        _policyResolver = policyResolver;
    }

    /// <inheritdoc/>
    public async Task<SlaTrendDto> Handle(GetSlaMonitorTrendQuery request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var windowStart = nowUtc.AddDays(-Math.Max(1, request.TrendWindowDays));

        var policies = await _policyResolver.ResolveAsync(request.TenantId, cancellationToken);

        var (windowIncidents, _) = await _incidentReadRepository.ListAsync(
            tenantId: request.TenantId,
            siteId: null,
            status: null,
            severity: null,
            occurredFromUtc: windowStart,
            occurredToUtc: nowUtc,
            orderBy: "IncidentOccurredAtUtc",
            sortDirection: "ASC",
            pageNumber: 1,
            pageSize: 5000,
            cancellationToken: cancellationToken);

        if (windowIncidents.Count == 0)
        {
            return new SlaTrendDto
            {
                CompliancePercentage = 100d,
                BreachCount = 0,
                SeverityDistribution = new Dictionary<string, int>(),
            };
        }

        var breachCount = 0;
        var severityDistribution = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in windowIncidents)
        {
            var policy = SlaPolicyResolver.Resolve(policies, row.Severity);
            var isResolvedOrArchived = row.Status is "Resolved" or "Archived";

            var result = IncidentSlaClockCalculator.Compute(
                incidentOccurredAtUtc: row.IncidentOccurredAtUtc,
                isAssigned: row.AssignedInvestigatorId.HasValue,
                isEscalated: row.Status == "Escalated",
                isResolvedOrArchived: isResolvedOrArchived,
                resolvedAtUtc: row.ClosedAtUtc,
                nowUtc: nowUtc,
                policy: policy);

            if (result.Status == SlaStatus.Breached)
                breachCount++;

            severityDistribution[row.Severity] = severityDistribution.GetValueOrDefault(row.Severity) + 1;
        }

        var compliancePercentage = 100d * (windowIncidents.Count - breachCount) / windowIncidents.Count;

        return new SlaTrendDto
        {
            CompliancePercentage = Math.Round(compliancePercentage, 2),
            BreachCount = breachCount,
            SeverityDistribution = severityDistribution,
        };
    }
}
