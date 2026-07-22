// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for AddWidgetToDashboardCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Repositories;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Handles <see cref="AddWidgetToDashboardCommand"/>.</summary>
public sealed class AddWidgetToDashboardCommandHandler : IRequestHandler<AddWidgetToDashboardCommand, Result<AddWidgetToDashboardResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<AddWidgetToDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddWidgetToDashboardCommandHandler(IDashboardRepository repository, ILogger<AddWidgetToDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AddWidgetToDashboardResult>> Handle(AddWidgetToDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new DashboardId(request.DashboardId), cancellationToken);
            if (aggregate is null)
            {
                return Result<AddWidgetToDashboardResult>.Failure(new ApplicationError(
                    "Dashboards.Dashboard.NotFound", $"Dashboard '{request.DashboardId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            var filterOverrides = request.FilterOverrides?
                .Select(kv => DashboardFilter.Create(kv.Key, kv.Value))
                .ToList();

            aggregate.AddWidget(
                new PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId(request.WidgetId),
                request.Row,
                request.Column,
                request.RowSpan,
                request.ColumnSpan,
                filterOverrides,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.AddedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AddWidgetToDashboardResult>.Success(new AddWidgetToDashboardResult(aggregate.DashboardId.Value, request.WidgetId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add Widget {WidgetId} to Dashboard {DashboardId}", request.WidgetId, request.DashboardId);
            return Result<AddWidgetToDashboardResult>.Failure(new ApplicationError(
                "Dashboards.Dashboard.AddWidgetFailed", "Unable to add the widget to the dashboard at this time.", request.CorrelationId));
        }
    }
}
