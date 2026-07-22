// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for PublishEmergencyResponsePlanningCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Interfaces;
using PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="PublishEmergencyResponsePlanningCommand"/>.</summary>
public sealed class PublishEmergencyResponsePlanningCommandHandler : IRequestHandler<PublishEmergencyResponsePlanningCommand, Result<EmergencyResponsePlanningActionResult>>
{
    private readonly IEmergencyResponsePlanningRepository _repository;
    private readonly ILogger<PublishEmergencyResponsePlanningCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishEmergencyResponsePlanningCommandHandler(IEmergencyResponsePlanningRepository repository, ILogger<PublishEmergencyResponsePlanningCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyResponsePlanningActionResult>> Handle(PublishEmergencyResponsePlanningCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EmergencyResponsePlanningId.FromGuid(request.EmergencyResponsePlanningId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EmergencyResponsePlanningActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.EmergencyResponsePlanning.NotFound",
                    "The requested EmergencyResponsePlanning record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Publish(
                UserId.FromGuid(request.ApprovedByUserId),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EmergencyResponsePlanningActionResult>.Success(new EmergencyResponsePlanningActionResult(
                aggregate.EmergencyResponsePlanningId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Publish for EmergencyResponsePlanning {EmergencyResponsePlanningId}", request.EmergencyResponsePlanningId);

            return Result<EmergencyResponsePlanningActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.EmergencyResponsePlanning.PublishFailed",
                "Unable to Publish the EmergencyResponsePlanning record at this time.",
                Guid.NewGuid()));
        }
    }
}