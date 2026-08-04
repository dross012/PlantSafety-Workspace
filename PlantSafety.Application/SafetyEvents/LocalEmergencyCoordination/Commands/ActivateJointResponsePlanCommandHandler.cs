// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ActivateJointResponsePlanCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateJointResponsePlanCommand. Mirrors RespondToEmergencyCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Handles <see cref="ActivateJointResponsePlanCommand"/>.</summary>
public sealed class ActivateJointResponsePlanCommandHandler : IRequestHandler<ActivateJointResponsePlanCommand, Result<JointResponsePlanStatusResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<ActivateJointResponsePlanCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateJointResponsePlanCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<ActivateJointResponsePlanCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<JointResponsePlanStatusResult>> Handle(ActivateJointResponsePlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LocalEmergencyCoordinationId.FromGuid(request.LocalEmergencyCoordinationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<JointResponsePlanStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    request.LocalEmergencyCoordinationId,
                    ErrorSeverity.NotFound));
            }

            var activatedBy = UserId.FromGuid(request.ActivatedByUserId);

            var plan = aggregate.ActivateJointResponsePlan(
                incidentId: request.IncidentId,
                planName: request.PlanName,
                activatedBy: activatedBy,
                activatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(activatedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<JointResponsePlanStatusResult>.Success(new JointResponsePlanStatusResult(
                aggregate.LocalEmergencyCoordinationId.Value, plan.JointResponsePlanId.Value, plan.IsActive));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate joint response plan for LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<JointResponsePlanStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.ActivateJointResponsePlanFailed",
                "Unable to activate the joint response plan at this time.",
                Guid.NewGuid()));
        }
    }
}
