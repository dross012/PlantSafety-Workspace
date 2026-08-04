// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           DeactivateJointResponsePlanCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for DeactivateJointResponsePlanCommand. Mirrors CloseEmergencyCommandHandler.
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

/// <summary>Handles <see cref="DeactivateJointResponsePlanCommand"/>.</summary>
public sealed class DeactivateJointResponsePlanCommandHandler : IRequestHandler<DeactivateJointResponsePlanCommand, Result<JointResponsePlanStatusResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<DeactivateJointResponsePlanCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeactivateJointResponsePlanCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<DeactivateJointResponsePlanCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<JointResponsePlanStatusResult>> Handle(DeactivateJointResponsePlanCommand request, CancellationToken cancellationToken)
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

            var deactivatedBy = UserId.FromGuid(request.DeactivatedByUserId);
            var planId = JointResponsePlanId.FromGuid(request.JointResponsePlanId);

            aggregate.DeactivateJointResponsePlan(
                jointResponsePlanId: planId,
                deactivatedBy: deactivatedBy,
                deactivatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(deactivatedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<JointResponsePlanStatusResult>.Success(new JointResponsePlanStatusResult(
                aggregate.LocalEmergencyCoordinationId.Value, request.JointResponsePlanId, false));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate joint response plan {JointResponsePlanId} for LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.JointResponsePlanId, request.LocalEmergencyCoordinationId);

            return Result<JointResponsePlanStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.DeactivateJointResponsePlanFailed",
                "Unable to deactivate the joint response plan at this time.",
                Guid.NewGuid()));
        }
    }
}
