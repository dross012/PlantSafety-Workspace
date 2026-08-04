// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListContainersByAccumulationAreaQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Queries;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories;

namespace PlantSafety.Application.HazardousWasteManagement.WasteContainers.Handlers;

/// <summary>Handles <see cref="ListContainersByAccumulationAreaQuery"/>.</summary>
public sealed class ListContainersByAccumulationAreaQueryHandler : IRequestHandler<ListContainersByAccumulationAreaQuery, Result<ListContainersByAccumulationAreaResult>>
{
    private readonly IWasteContainerRepository _repository;
    private readonly ILogger<ListContainersByAccumulationAreaQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListContainersByAccumulationAreaQueryHandler(IWasteContainerRepository repository, ILogger<ListContainersByAccumulationAreaQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListContainersByAccumulationAreaResult>> Handle(ListContainersByAccumulationAreaQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListContainersByAccumulationAreaAsync(request.TenantId, request.AccumulationAreaId, cancellationToken);
            return Result<ListContainersByAccumulationAreaResult>.Success(new ListContainersByAccumulationAreaResult(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list WasteContainers for AccumulationArea {AccumulationAreaId}", request.AccumulationAreaId);

            return Result<ListContainersByAccumulationAreaResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.WasteContainer.ListFailed",
                "Unable to list accumulation containers at this time.",
                Guid.NewGuid()));
        }
    }
}
