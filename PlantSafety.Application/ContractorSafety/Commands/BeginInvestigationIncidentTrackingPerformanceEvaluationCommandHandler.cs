// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for BeginInvestigationIncidentTrackingPerformanceEvaluationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Interfaces;
using PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Handles <see cref="BeginInvestigationIncidentTrackingPerformanceEvaluationCommand"/>.</summary>
public sealed class BeginInvestigationIncidentTrackingPerformanceEvaluationCommandHandler : IRequestHandler<BeginInvestigationIncidentTrackingPerformanceEvaluationCommand, Result<IncidentTrackingPerformanceEvaluationActionResult>>
{
    private readonly IIncidentTrackingPerformanceEvaluationRepository _repository;
    private readonly ILogger<BeginInvestigationIncidentTrackingPerformanceEvaluationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginInvestigationIncidentTrackingPerformanceEvaluationCommandHandler(IIncidentTrackingPerformanceEvaluationRepository repository, ILogger<BeginInvestigationIncidentTrackingPerformanceEvaluationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentTrackingPerformanceEvaluationActionResult>> Handle(BeginInvestigationIncidentTrackingPerformanceEvaluationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = IncidentTrackingPerformanceEvaluationId.FromGuid(request.IncidentTrackingPerformanceEvaluationId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<IncidentTrackingPerformanceEvaluationActionResult>.Failure(new ApplicationError(
                    "ContractorSafety.IncidentTrackingPerformanceEvaluation.NotFound",
                    "The requested IncidentTrackingPerformanceEvaluation record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.BeginInvestigation(
                UserId.FromGuid(request.EvaluatedByUserId),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<IncidentTrackingPerformanceEvaluationActionResult>.Success(new IncidentTrackingPerformanceEvaluationActionResult(
                aggregate.IncidentTrackingPerformanceEvaluationId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute BeginInvestigation for IncidentTrackingPerformanceEvaluation {IncidentTrackingPerformanceEvaluationId}", request.IncidentTrackingPerformanceEvaluationId);

            return Result<IncidentTrackingPerformanceEvaluationActionResult>.Failure(new ApplicationError(
                "ContractorSafety.IncidentTrackingPerformanceEvaluation.BeginInvestigationFailed",
                "Unable to BeginInvestigation the IncidentTrackingPerformanceEvaluation record at this time.",
                Guid.NewGuid()));
        }
    }
}