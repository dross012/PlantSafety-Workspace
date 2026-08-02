// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           CompleteEmergencyDrillPlanningCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteEmergencyDrillPlanningCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Handles <see cref="CompleteEmergencyDrillPlanningCommand"/>.</summary>
public sealed class CompleteEmergencyDrillPlanningCommandHandler : IRequestHandler<CompleteEmergencyDrillPlanningCommand, Result<EmergencyDrillPlanningStatusResult>>
{
    private readonly IEmergencyDrillPlanningRepository _repository;
    private readonly ILogger<CompleteEmergencyDrillPlanningCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteEmergencyDrillPlanningCommandHandler(IEmergencyDrillPlanningRepository repository, ILogger<CompleteEmergencyDrillPlanningCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyDrillPlanningStatusResult>> Handle(CompleteEmergencyDrillPlanningCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EmergencyDrillPlanningId.FromGuid(request.EmergencyDrillPlanningId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EmergencyDrillPlanningStatusResult>.Failure(new ApplicationError(
                    "Scheduling.EmergencyDrillPlanning.NotFound",
                    "The requested emergency drill could not be found.",
                    request.EmergencyDrillPlanningId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Complete(
                completedBy: performedBy,
                debriefNotes: request.DebriefNotes,
                completedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EmergencyDrillPlanningStatusResult>.Success(new EmergencyDrillPlanningStatusResult(
                aggregate.EmergencyDrillPlanningId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete EmergencyDrillPlanning {EmergencyDrillPlanningId}", request.EmergencyDrillPlanningId);

            return Result<EmergencyDrillPlanningStatusResult>.Failure(new ApplicationError(
                "Scheduling.EmergencyDrillPlanning.CompleteFailed",
                "Unable to complete the emergency drill at this time.",
                Guid.NewGuid()));
        }
    }
}
