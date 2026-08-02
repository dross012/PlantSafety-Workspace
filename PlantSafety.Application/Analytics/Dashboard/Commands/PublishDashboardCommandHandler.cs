// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           PublishDashboardCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for PublishDashboardCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.Dashboard.Interfaces;
using PlantSafety.Domain.Analytics.Dashboard.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Dashboard.Commands;

/// <summary>Handles <see cref="PublishDashboardCommand"/>.</summary>
public sealed class PublishDashboardCommandHandler : IRequestHandler<PublishDashboardCommand, Result<DashboardStatusResult>>
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
    public async Task<Result<DashboardStatusResult>> Handle(PublishDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DashboardId.FromGuid(request.DashboardId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<DashboardStatusResult>.Failure(new ApplicationError(
                    "Analytics.Dashboard.NotFound",
                    "The requested dashboard could not be found.",
                    request.DashboardId,
                    ErrorSeverity.NotFound));
            }

            var publishedBy = UserId.FromGuid(request.PublishedByUserId);

            aggregate.Publish(
                publishedBy: publishedBy,
                publishedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(publishedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<DashboardStatusResult>.Success(new DashboardStatusResult(
                aggregate.DashboardId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish Dashboard {DashboardId}", request.DashboardId);

            return Result<DashboardStatusResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.PublishFailed",
                "Unable to publish the dashboard at this time.",
                Guid.NewGuid()));
        }
    }
}
