// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           ReviewPredictiveMaintenanceAlertCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ReviewPredictiveMaintenanceAlertCommand. GetByIdAsync is
//                  tenant-scoped, the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Handles <see cref="ReviewPredictiveMaintenanceAlertCommand"/>.</summary>
public sealed class ReviewPredictiveMaintenanceAlertCommandHandler : IRequestHandler<ReviewPredictiveMaintenanceAlertCommand, Result<PredictiveMaintenanceAlertStatusResult>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<ReviewPredictiveMaintenanceAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReviewPredictiveMaintenanceAlertCommandHandler(IPredictiveMaintenanceAlertRepository repository, ILogger<ReviewPredictiveMaintenanceAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceAlertStatusResult>> Handle(ReviewPredictiveMaintenanceAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PredictiveMaintenanceAlertId.FromGuid(request.PredictiveMaintenanceAlertId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.NotFound",
                    "The requested predictive maintenance alert could not be found.",
                    request.PredictiveMaintenanceAlertId,
                    ErrorSeverity.NotFound));
            }

            var reviewedBy = UserId.FromGuid(request.ReviewedByUserId);

            aggregate.Review(
                reviewedBy: reviewedBy,
                reviewedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(reviewedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PredictiveMaintenanceAlertStatusResult>.Success(new PredictiveMaintenanceAlertStatusResult(
                aggregate.PredictiveMaintenanceAlertId.Value,
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
            _logger.LogError(ex, "Failed to review PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId}", request.PredictiveMaintenanceAlertId);

            return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.ReviewFailed",
                "Unable to review the predictive maintenance alert at this time.",
                Guid.NewGuid()));
        }
    }
}
