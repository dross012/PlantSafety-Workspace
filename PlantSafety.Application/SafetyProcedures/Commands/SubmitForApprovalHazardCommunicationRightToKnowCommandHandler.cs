// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for SubmitForApprovalHazardCommunicationRightToKnowCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Interfaces;
using PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="SubmitForApprovalHazardCommunicationRightToKnowCommand"/>.</summary>
public sealed class SubmitForApprovalHazardCommunicationRightToKnowCommandHandler : IRequestHandler<SubmitForApprovalHazardCommunicationRightToKnowCommand, Result<HazardCommunicationRightToKnowActionResult>>
{
    private readonly IHazardCommunicationRightToKnowRepository _repository;
    private readonly ILogger<SubmitForApprovalHazardCommunicationRightToKnowCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitForApprovalHazardCommunicationRightToKnowCommandHandler(IHazardCommunicationRightToKnowRepository repository, ILogger<SubmitForApprovalHazardCommunicationRightToKnowCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationRightToKnowActionResult>> Handle(SubmitForApprovalHazardCommunicationRightToKnowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazardCommunicationRightToKnowId.FromGuid(request.HazardCommunicationRightToKnowId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazardCommunicationRightToKnowActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.HazardCommunicationRightToKnow.NotFound",
                    "The requested HazardCommunicationRightToKnow record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.SubmitForApproval(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<HazardCommunicationRightToKnowActionResult>.Success(new HazardCommunicationRightToKnowActionResult(
                aggregate.HazardCommunicationRightToKnowId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute SubmitForApproval for HazardCommunicationRightToKnow {HazardCommunicationRightToKnowId}", request.HazardCommunicationRightToKnowId);

            return Result<HazardCommunicationRightToKnowActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.HazardCommunicationRightToKnow.SubmitForApprovalFailed",
                "Unable to SubmitForApproval the HazardCommunicationRightToKnow record at this time.",
                Guid.NewGuid()));
        }
    }
}