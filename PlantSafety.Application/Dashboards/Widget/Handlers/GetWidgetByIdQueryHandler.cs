// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetWidgetByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Models;
using PlantSafety.Application.Dashboards.Widget.Queries;
using PlantSafety.Application.Dashboards.Widget.Repositories;

namespace PlantSafety.Application.Dashboards.Widget.Handlers;

/// <summary>Handles <see cref="GetWidgetByIdQuery"/>.</summary>
public sealed class GetWidgetByIdQueryHandler : IRequestHandler<GetWidgetByIdQuery, Result<WidgetDto>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<GetWidgetByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWidgetByIdQueryHandler(IWidgetRepository repository, ILogger<GetWidgetByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WidgetDto>> Handle(GetWidgetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var widget = await _repository.GetDtoByIdAsync(request.TenantId, request.WidgetId, cancellationToken);

            return widget is null
                ? Result<WidgetDto>.Failure(new ApplicationError(
                    "Dashboards.Widget.NotFound", $"Widget '{request.WidgetId}' was not found.", Guid.NewGuid(), ErrorSeverity.NotFound))
                : Result<WidgetDto>.Success(widget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Widget {WidgetId} for Tenant {TenantId}", request.WidgetId, request.TenantId);

            return Result<WidgetDto>.Failure(new ApplicationError(
                "Dashboards.Widget.RetrievalFailed", "Unable to retrieve the widget at this time.", Guid.NewGuid()));
        }
    }
}
