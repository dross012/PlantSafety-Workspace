// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           StartReviewNearMissesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StartReviewNearMissesCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Commands
//                  .ReviewPredictiveMaintenanceAlertCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces;
using PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Commands;

/// <summary>Handles <see cref="StartReviewNearMissesCommand"/>.</summary>
public sealed class StartReviewNearMissesCommandHandler : IRequestHandler<StartReviewNearMissesCommand, Result<NearMissesStatusResult>>
{
    private readonly INearMissesRepository _repository;
    private readonly ILogger<StartReviewNearMissesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartReviewNearMissesCommandHandler(INearMissesRepository repository, ILogger<StartReviewNearMissesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissesStatusResult>> Handle(StartReviewNearMissesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = NearMissesId.FromGuid(request.NearMissesId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<NearMissesStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.NearMisses.NotFound",
                    "The requested near miss could not be found.",
                    request.NearMissesId,
                    ErrorSeverity.NotFound));
            }

            var reviewedBy = UserId.FromGuid(request.ReviewedByUserId);

            aggregate.StartReview(
                reviewedBy: reviewedBy,
                reviewStartedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(reviewedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<NearMissesStatusResult>.Success(new NearMissesStatusResult(
                aggregate.NearMissesId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409 --
            // swallowing it here as a generic ApplicationError would misreport a stale-ETag
            // conflict as an ordinary 400 failure.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start review of NearMisses {NearMissesId}", request.NearMissesId);

            return Result<NearMissesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.NearMisses.StartReviewFailed",
                "Unable to start review of the near miss at this time.",
                Guid.NewGuid()));
        }
    }
}
