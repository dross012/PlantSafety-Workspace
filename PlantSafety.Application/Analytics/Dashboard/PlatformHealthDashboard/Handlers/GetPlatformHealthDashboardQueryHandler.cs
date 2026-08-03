// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetPlatformHealthDashboardQuery. Composes the new
//                    IPlatformHealthDashboardRepository (request throughput/latency/error rate,
//                    backed by the new [PlatformHealth].[RequestMetricEvent] table) with the
//                    EXISTING IDeadLetterEventRepository (event-bus/DLQ health) -- reusing real
//                    Phase 6 dead-letter analytics infrastructure rather than duplicating it.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Handlers;

/// <summary>Handles <see cref="GetPlatformHealthDashboardQuery"/>.</summary>
public sealed class GetPlatformHealthDashboardQueryHandler(
    IPlatformHealthDashboardRepository repository,
    IDeadLetterEventRepository deadLetterEventRepository,
    ILogger<GetPlatformHealthDashboardQueryHandler> logger)
    : IRequestHandler<GetPlatformHealthDashboardQuery, Result<PlatformHealthDashboardQueryResult>>
{
    /// <inheritdoc/>
    public async Task<Result<PlatformHealthDashboardQueryResult>> Handle(GetPlatformHealthDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await repository.GetPlatformHealthDashboardAsync(request.TenantId, request.From, request.To, cancellationToken);

            var eventBusHealth = await deadLetterEventRepository.GetSummaryAsync(
                request.TenantId.ToString(), request.DeadLetterLookbackDays, cancellationToken);

            return Result<PlatformHealthDashboardQueryResult>.Success(new PlatformHealthDashboardQueryResult(
                request.TenantId,
                DateTime.UtcNow,
                items,
                eventBusHealth));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve PlatformHealthDashboard for Tenant {TenantId}", request.TenantId);

            return Result<PlatformHealthDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.PlatformHealthDashboard.RetrievalFailed",
                "Unable to retrieve Platform Health dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
