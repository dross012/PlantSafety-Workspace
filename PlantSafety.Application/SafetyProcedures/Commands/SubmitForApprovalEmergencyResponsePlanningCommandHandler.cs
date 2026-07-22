// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for SubmitForApprovalEmergencyResponsePlanningCommand.
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

/// <summary>Handles <see cref="SubmitForApprovalEmergencyResponsePlanningCommand"/>.</summary>
public sealed class SubmitForApprovalEmergencyResponsePlanningCommandHandler : IRequestHandler<SubmitForApprovalEmergencyResponsePlanningCommand, Result<EmergencyResponsePlanningActionResult>>
{
    private readonly IEmergencyResponsePlanningRepository _repository;
    private readonly ILogger<SubmitForApprovalEmergencyResponsePlanningCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitForApprovalEmergencyResponsePlanningCommandHandler(IEmergencyResponsePlanningRepository repository, ILogger<SubmitForApprovalEmergencyResponsePlanningCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyResponsePlanningActionResult>> Handle(SubmitForApprovalEmergencyResponsePlanningCommand request, CancellationToken cancellationToken)
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

            aggregate.SubmitForApproval(
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
            _logger.LogError(ex, "Failed to execute SubmitForApproval for EmergencyResponsePlanning {EmergencyResponsePlanningId}", request.EmergencyResponsePlanningId);

            return Result<EmergencyResponsePlanningActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.EmergencyResponsePlanning.SubmitForApprovalFailed",
                "Unable to SubmitForApproval the EmergencyResponsePlanning record at this time.",
                Guid.NewGuid()));
        }
    }
}