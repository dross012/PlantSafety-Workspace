// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSafetyDashboardQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Analytics.Models.Dashboard;
using PlantSafety.Application.Features.Analytics.Queries.Dashboard;

namespace PlantSafety.Application.Features.Analytics.Handlers.Dashboard;

/// <summary>Handles <see cref="GetSafetyDashboardQuery"/>. Returns null when no data exists for the tenant.</summary>
public sealed class GetSafetyDashboardQueryHandler : IQueryHandler<GetSafetyDashboardQuery, SafetyDashboardDto?>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<GetSafetyDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyDashboardQueryHandler(IDashboardRepository repository, ILogger<GetSafetyDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SafetyDashboardDto?> Handle(GetSafetyDashboardQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetDashboardAsync(request.TenantId, request.SiteId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Dashboard data not found for Tenant {TenantId} SiteId {SiteId}", request.TenantId, request.SiteId);
            return null;
        }

        return new SafetyDashboardDto
        {
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            TotalIncidents = model.TotalIncidents,
            OpenIncidents = model.OpenIncidents,
            CriticalIncidents = model.CriticalIncidents,
            LostTimeInjuries = model.LostTimeInjuries,
            TotalNearMisses = model.TotalNearMisses,
            TotalObservations = model.TotalObservations,
            PositiveObservations = model.PositiveObservations,
            NegativeObservations = model.NegativeObservations,
            OpenPermits = model.OpenPermits,
            OverdueObligations = model.OverdueObligations,
            CultureScore = model.CultureScore,
            CalculatedAtUtc = model.CalculatedAtUtc,
        };
    }
}
