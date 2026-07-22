// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordHazardAssessmentPermitToWorkCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorSafety.PermitToWork.Interfaces;
using PlantSafety.Domain.ContractorSafety.PermitToWork.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Handles <see cref="RecordHazardAssessmentPermitToWorkCommand"/>.</summary>
public sealed class RecordHazardAssessmentPermitToWorkCommandHandler : IRequestHandler<RecordHazardAssessmentPermitToWorkCommand, Result<PermitToWorkActionResult>>
{
    private readonly IPermitToWorkRepository _repository;
    private readonly ILogger<RecordHazardAssessmentPermitToWorkCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordHazardAssessmentPermitToWorkCommandHandler(IPermitToWorkRepository repository, ILogger<RecordHazardAssessmentPermitToWorkCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitToWorkActionResult>> Handle(RecordHazardAssessmentPermitToWorkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PermitToWorkId.FromGuid(request.PermitToWorkId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PermitToWorkActionResult>.Failure(new ApplicationError(
                    "ContractorSafety.PermitToWork.NotFound",
                    "The requested PermitToWork record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordHazardAssessment(
                request.HazardCount,
                request.AtmosphericTestScore,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PermitToWorkActionResult>.Success(new PermitToWorkActionResult(
                aggregate.PermitToWorkId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordHazardAssessment for PermitToWork {PermitToWorkId}", request.PermitToWorkId);

            return Result<PermitToWorkActionResult>.Failure(new ApplicationError(
                "ContractorSafety.PermitToWork.RecordHazardAssessmentFailed",
                "Unable to RecordHazardAssessment the PermitToWork record at this time.",
                Guid.NewGuid()));
        }
    }
}