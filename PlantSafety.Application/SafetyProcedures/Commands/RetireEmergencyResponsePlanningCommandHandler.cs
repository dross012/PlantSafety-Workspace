// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RetireEmergencyResponsePlanningCommand.
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

/// <summary>Handles <see cref="RetireEmergencyResponsePlanningCommand"/>.</summary>
public sealed class RetireEmergencyResponsePlanningCommandHandler : IRequestHandler<RetireEmergencyResponsePlanningCommand, Result<EmergencyResponsePlanningActionResult>>
{
    private readonly IEmergencyResponsePlanningRepository _repository;
    private readonly ILogger<RetireEmergencyResponsePlanningCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireEmergencyResponsePlanningCommandHandler(IEmergencyResponsePlanningRepository repository, ILogger<RetireEmergencyResponsePlanningCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyResponsePlanningActionResult>> Handle(RetireEmergencyResponsePlanningCommand request, CancellationToken cancellationToken)
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

            aggregate.Retire(
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
            _logger.LogError(ex, "Failed to execute Retire for EmergencyResponsePlanning {EmergencyResponsePlanningId}", request.EmergencyResponsePlanningId);

            return Result<EmergencyResponsePlanningActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.EmergencyResponsePlanning.RetireFailed",
                "Unable to Retire the EmergencyResponsePlanning record at this time.",
                Guid.NewGuid()));
        }
    }
}