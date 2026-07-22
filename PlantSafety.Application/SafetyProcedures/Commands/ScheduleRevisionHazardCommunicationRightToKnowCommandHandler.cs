// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ScheduleRevisionHazardCommunicationRightToKnowCommand.
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

/// <summary>Handles <see cref="ScheduleRevisionHazardCommunicationRightToKnowCommand"/>.</summary>
public sealed class ScheduleRevisionHazardCommunicationRightToKnowCommandHandler : IRequestHandler<ScheduleRevisionHazardCommunicationRightToKnowCommand, Result<HazardCommunicationRightToKnowActionResult>>
{
    private readonly IHazardCommunicationRightToKnowRepository _repository;
    private readonly ILogger<ScheduleRevisionHazardCommunicationRightToKnowCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleRevisionHazardCommunicationRightToKnowCommandHandler(IHazardCommunicationRightToKnowRepository repository, ILogger<ScheduleRevisionHazardCommunicationRightToKnowCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationRightToKnowActionResult>> Handle(ScheduleRevisionHazardCommunicationRightToKnowCommand request, CancellationToken cancellationToken)
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

            aggregate.ScheduleRevision(
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
            _logger.LogError(ex, "Failed to execute ScheduleRevision for HazardCommunicationRightToKnow {HazardCommunicationRightToKnowId}", request.HazardCommunicationRightToKnowId);

            return Result<HazardCommunicationRightToKnowActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.HazardCommunicationRightToKnow.ScheduleRevisionFailed",
                "Unable to ScheduleRevision the HazardCommunicationRightToKnow record at this time.",
                Guid.NewGuid()));
        }
    }
}