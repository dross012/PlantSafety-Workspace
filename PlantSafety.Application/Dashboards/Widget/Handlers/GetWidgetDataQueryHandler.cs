// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetWidgetDataQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Models;
using PlantSafety.Application.Dashboards.Widget.Queries;
using PlantSafety.Application.Dashboards.Widget.Repositories;

namespace PlantSafety.Application.Dashboards.Widget.Handlers;

/// <summary>Handles <see cref="GetWidgetDataQuery"/>.</summary>
public sealed class GetWidgetDataQueryHandler : IRequestHandler<GetWidgetDataQuery, Result<WidgetDataContractDto>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<GetWidgetDataQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWidgetDataQueryHandler(IWidgetRepository repository, ILogger<GetWidgetDataQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WidgetDataContractDto>> Handle(GetWidgetDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var contract = await _repository.GetWidgetDataAsync(
                request.TenantId, request.WidgetId, request.FilterOverrides ?? new Dictionary<string, string>(), cancellationToken);

            return Result<WidgetDataContractDto>.Success(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve data for Widget {WidgetId} and Tenant {TenantId}", request.WidgetId, request.TenantId);

            return Result<WidgetDataContractDto>.Failure(new ApplicationError(
                "Dashboards.Widget.DataResolutionFailed", "Unable to resolve widget data at this time.", Guid.NewGuid()));
        }
    }
}
