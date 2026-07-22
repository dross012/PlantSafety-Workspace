// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for ActivateWidgetCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Repositories;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Handles <see cref="ActivateWidgetCommand"/>.</summary>
public sealed class ActivateWidgetCommandHandler : IRequestHandler<ActivateWidgetCommand, Result<ActivateWidgetResult>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<ActivateWidgetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateWidgetCommandHandler(IWidgetRepository repository, ILogger<ActivateWidgetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ActivateWidgetResult>> Handle(ActivateWidgetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new WidgetId(request.WidgetId), cancellationToken);
            if (aggregate is null)
            {
                return Result<ActivateWidgetResult>.Failure(new ApplicationError(
                    "Dashboards.Widget.NotFound", $"Widget '{request.WidgetId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Activate(
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.ActivatedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ActivateWidgetResult>.Success(new ActivateWidgetResult(aggregate.WidgetId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate Widget {WidgetId}", request.WidgetId);
            return Result<ActivateWidgetResult>.Failure(new ApplicationError(
                "Dashboards.Widget.ActivateFailed", "Unable to activate the widget at this time.", request.CorrelationId));
        }
    }
}
