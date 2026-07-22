// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetWidgetLibraryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Models;
using PlantSafety.Application.Dashboards.Widget.Queries;
using PlantSafety.Application.Dashboards.Widget.Repositories;

namespace PlantSafety.Application.Dashboards.Widget.Handlers;

/// <summary>Handles <see cref="GetWidgetLibraryQuery"/>.</summary>
public sealed class GetWidgetLibraryQueryHandler : IRequestHandler<GetWidgetLibraryQuery, Result<IReadOnlyList<WidgetDto>>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<GetWidgetLibraryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWidgetLibraryQueryHandler(IWidgetRepository repository, ILogger<GetWidgetLibraryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WidgetDto>>> Handle(GetWidgetLibraryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var widgets = await _repository.GetWidgetLibraryAsync(request.TenantId, cancellationToken);
            return Result<IReadOnlyList<WidgetDto>>.Success(widgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve widget library for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<WidgetDto>>.Failure(new ApplicationError(
                "Dashboards.Widget.LibraryRetrievalFailed", "Unable to retrieve the widget library at this time.", Guid.NewGuid()));
        }
    }
}
