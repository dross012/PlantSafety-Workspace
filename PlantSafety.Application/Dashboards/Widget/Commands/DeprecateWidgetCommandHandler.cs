// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for DeprecateWidgetCommand.
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

/// <summary>Handles <see cref="DeprecateWidgetCommand"/>.</summary>
public sealed class DeprecateWidgetCommandHandler : IRequestHandler<DeprecateWidgetCommand, Result<DeprecateWidgetResult>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<DeprecateWidgetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeprecateWidgetCommandHandler(IWidgetRepository repository, ILogger<DeprecateWidgetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DeprecateWidgetResult>> Handle(DeprecateWidgetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new WidgetId(request.WidgetId), cancellationToken);
            if (aggregate is null)
            {
                return Result<DeprecateWidgetResult>.Failure(new ApplicationError(
                    "Dashboards.Widget.NotFound", $"Widget '{request.WidgetId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Deprecate(
                request.Reason,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.DeprecatedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DeprecateWidgetResult>.Success(new DeprecateWidgetResult(aggregate.WidgetId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deprecate Widget {WidgetId}", request.WidgetId);
            return Result<DeprecateWidgetResult>.Failure(new ApplicationError(
                "Dashboards.Widget.DeprecateFailed", "Unable to deprecate the widget at this time.", request.CorrelationId));
        }
    }
}
