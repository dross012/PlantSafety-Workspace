// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for RemoveWidgetFromDashboardCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Repositories;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Handles <see cref="RemoveWidgetFromDashboardCommand"/>.</summary>
public sealed class RemoveWidgetFromDashboardCommandHandler : IRequestHandler<RemoveWidgetFromDashboardCommand, Result<RemoveWidgetFromDashboardResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<RemoveWidgetFromDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RemoveWidgetFromDashboardCommandHandler(IDashboardRepository repository, ILogger<RemoveWidgetFromDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RemoveWidgetFromDashboardResult>> Handle(RemoveWidgetFromDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new DashboardId(request.DashboardId), cancellationToken);
            if (aggregate is null)
            {
                return Result<RemoveWidgetFromDashboardResult>.Failure(new ApplicationError(
                    "Dashboards.Dashboard.NotFound", $"Dashboard '{request.DashboardId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.RemoveWidget(
                new PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId(request.WidgetId),
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.RemovedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RemoveWidgetFromDashboardResult>.Success(new RemoveWidgetFromDashboardResult(aggregate.DashboardId.Value, request.WidgetId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove Widget {WidgetId} from Dashboard {DashboardId}", request.WidgetId, request.DashboardId);
            return Result<RemoveWidgetFromDashboardResult>.Failure(new ApplicationError(
                "Dashboards.Dashboard.RemoveWidgetFailed", "Unable to remove the widget from the dashboard at this time.", request.CorrelationId));
        }
    }
}
