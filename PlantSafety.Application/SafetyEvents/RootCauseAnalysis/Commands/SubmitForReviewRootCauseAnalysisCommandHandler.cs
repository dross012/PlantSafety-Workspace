// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           SubmitForReviewRootCauseAnalysisCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for SubmitForReviewRootCauseAnalysisCommand. GetByIdAsync is
//                  tenant-scoped, the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;

/// <summary>Handles <see cref="SubmitForReviewRootCauseAnalysisCommand"/>.</summary>
public sealed class SubmitForReviewRootCauseAnalysisCommandHandler : IRequestHandler<SubmitForReviewRootCauseAnalysisCommand, Result<RootCauseAnalysisStatusResult>>
{
    private readonly IRootCauseAnalysisRepository _repository;
    private readonly ILogger<SubmitForReviewRootCauseAnalysisCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitForReviewRootCauseAnalysisCommandHandler(IRootCauseAnalysisRepository repository, ILogger<SubmitForReviewRootCauseAnalysisCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RootCauseAnalysisStatusResult>> Handle(SubmitForReviewRootCauseAnalysisCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RootCauseAnalysisId.FromGuid(request.RootCauseAnalysisId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RootCauseAnalysisStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.RootCauseAnalysis.NotFound",
                    "The requested root cause analysis could not be found.",
                    request.RootCauseAnalysisId,
                    ErrorSeverity.NotFound));
            }

            var submittedBy = UserId.FromGuid(request.SubmittedByUserId);

            aggregate.SubmitForReview(
                submittedBy: submittedBy,
                submittedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(submittedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RootCauseAnalysisStatusResult>.Success(new RootCauseAnalysisStatusResult(
                aggregate.RootCauseAnalysisId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit RootCauseAnalysis {RootCauseAnalysisId} for review", request.RootCauseAnalysisId);

            return Result<RootCauseAnalysisStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.RootCauseAnalysis.SubmitForReviewFailed",
                "Unable to submit the root cause analysis for review at this time.",
                Guid.NewGuid()));
        }
    }
}
