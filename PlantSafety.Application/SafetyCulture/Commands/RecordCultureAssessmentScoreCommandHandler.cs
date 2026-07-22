// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordCultureAssessmentScoreCommand. Pure orchestration: loads the CultureAssessment aggregate, invokes its RecordScore() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Handles <see cref="RecordCultureAssessmentScoreCommand"/>.</summary>
public sealed class RecordCultureAssessmentScoreCommandHandler : IRequestHandler<RecordCultureAssessmentScoreCommand, Result<RecordCultureAssessmentScoreResult>>
{
    private readonly ICultureAssessmentRepository _repository;
    private readonly ILogger<RecordCultureAssessmentScoreCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordCultureAssessmentScoreCommandHandler(ICultureAssessmentRepository repository, ILogger<RecordCultureAssessmentScoreCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordCultureAssessmentScoreResult>> Handle(RecordCultureAssessmentScoreCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CultureAssessmentId.FromGuid(request.CultureAssessmentId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<RecordCultureAssessmentScoreResult>.Failure(new ApplicationError(
                    "SafetyCulture.CultureAssessment.NotFound",
                    "The requested CultureAssessment record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.RecordScore(
                overallCultureScore: request.OverallCultureScore,
                leadershipEngagementScore: request.LeadershipEngagementScore,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RecordCultureAssessmentScoreResult>.Success(new RecordCultureAssessmentScoreResult(
                aggregate.CultureAssessmentId.Value,
                aggregate.Status.ToString(),
                aggregate.OverallCultureScore!.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record score on CultureAssessment {CultureAssessmentId}", request.CultureAssessmentId);

            return Result<RecordCultureAssessmentScoreResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.RecordScoreFailed",
                "Unable to record the culture score at this time.",
                request.CorrelationId));
        }
    }
}
