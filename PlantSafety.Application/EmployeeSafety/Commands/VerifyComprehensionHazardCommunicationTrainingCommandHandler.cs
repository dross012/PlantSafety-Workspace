// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for VerifyComprehensionHazardCommunicationTrainingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Interfaces;
using PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="VerifyComprehensionHazardCommunicationTrainingCommand"/>.</summary>
public sealed class VerifyComprehensionHazardCommunicationTrainingCommandHandler : IRequestHandler<VerifyComprehensionHazardCommunicationTrainingCommand, Result<HazardCommunicationTrainingActionResult>>
{
    private readonly IHazardCommunicationTrainingRepository _repository;
    private readonly ILogger<VerifyComprehensionHazardCommunicationTrainingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VerifyComprehensionHazardCommunicationTrainingCommandHandler(IHazardCommunicationTrainingRepository repository, ILogger<VerifyComprehensionHazardCommunicationTrainingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationTrainingActionResult>> Handle(VerifyComprehensionHazardCommunicationTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazardCommunicationTrainingId.FromGuid(request.HazardCommunicationTrainingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazardCommunicationTrainingActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.HazardCommunicationTraining.NotFound",
                    "The requested HazardCommunicationTraining record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.VerifyComprehension(
                request.ComprehensionScore,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<HazardCommunicationTrainingActionResult>.Success(new HazardCommunicationTrainingActionResult(
                aggregate.HazardCommunicationTrainingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute VerifyComprehension for HazardCommunicationTraining {HazardCommunicationTrainingId}", request.HazardCommunicationTrainingId);

            return Result<HazardCommunicationTrainingActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.HazardCommunicationTraining.VerifyComprehensionFailed",
                "Unable to VerifyComprehension the HazardCommunicationTraining record at this time.",
                Guid.NewGuid()));
        }
    }
}