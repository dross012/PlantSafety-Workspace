// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetBottleneckHistoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Repositories;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Handlers;

/// <summary>Handles <see cref="GetBottleneckHistoryQuery"/>.</summary>
public sealed class GetBottleneckHistoryQueryHandler : IRequestHandler<GetBottleneckHistoryQuery, Result<IReadOnlyList<EvacuationBottleneckItemDto>>>
{
    private readonly IEvacuationBottleneckRepository _repository;
    private readonly ILogger<GetBottleneckHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBottleneckHistoryQueryHandler(IEvacuationBottleneckRepository repository, ILogger<GetBottleneckHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EvacuationBottleneckItemDto>>> Handle(GetBottleneckHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHistoryAsync(request.TenantId, request.SiteId, request.RouteId, cancellationToken);
            return Result<IReadOnlyList<EvacuationBottleneckItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve bottleneck history for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<EvacuationBottleneckItemDto>>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationBottleneck.HistoryRetrievalFailed",
                "Unable to retrieve bottleneck history at this time.",
                Guid.NewGuid()));
        }
    }
}
