// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for PublishDashboardCommand.
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

/// <summary>Handles <see cref="PublishDashboardCommand"/>.</summary>
public sealed class PublishDashboardCommandHandler : IRequestHandler<PublishDashboardCommand, Result<PublishDashboardResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<PublishDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishDashboardCommandHandler(IDashboardRepository repository, ILogger<PublishDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PublishDashboardResult>> Handle(PublishDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new DashboardId(request.DashboardId), cancellationToken);
            if (aggregate is null)
            {
                return Result<PublishDashboardResult>.Failure(new ApplicationError(
                    "Dashboards.Dashboard.NotFound", $"Dashboard '{request.DashboardId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Publish(
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.PublishedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PublishDashboardResult>.Success(new PublishDashboardResult(aggregate.DashboardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish Dashboard {DashboardId}", request.DashboardId);
            return Result<PublishDashboardResult>.Failure(new ApplicationError(
                "Dashboards.Dashboard.PublishFailed", "Unable to publish the dashboard at this time.", request.CorrelationId));
        }
    }
}
