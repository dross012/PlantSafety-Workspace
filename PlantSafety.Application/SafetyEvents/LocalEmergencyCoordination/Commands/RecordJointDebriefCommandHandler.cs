// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           RecordJointDebriefCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordJointDebriefCommand. Mirrors RespondToEmergencyCommandHandler.
//                  Backs the "joint debrief completion rate" KPI once persisted.
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

/// <summary>Handles <see cref="RecordJointDebriefCommand"/>.</summary>
public sealed class RecordJointDebriefCommandHandler : IRequestHandler<RecordJointDebriefCommand, Result<RecordJointDebriefResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<RecordJointDebriefCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordJointDebriefCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<RecordJointDebriefCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordJointDebriefResult>> Handle(RecordJointDebriefCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LocalEmergencyCoordinationId.FromGuid(request.LocalEmergencyCoordinationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RecordJointDebriefResult>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    request.LocalEmergencyCoordinationId,
                    ErrorSeverity.NotFound));
            }

            var recordedBy = UserId.FromGuid(request.RecordedByUserId);

            var debrief = aggregate.RecordJointDebrief(
                incidentId: request.IncidentId,
                agenciesInvolved: request.AgenciesInvolved,
                summary: request.Summary,
                recordedBy: recordedBy,
                recordedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(recordedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RecordJointDebriefResult>.Success(new RecordJointDebriefResult(
                aggregate.LocalEmergencyCoordinationId.Value, debrief.JointDebriefRecordId.Value));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record joint debrief for LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<RecordJointDebriefResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.RecordJointDebriefFailed",
                "Unable to record the joint debrief at this time.",
                Guid.NewGuid()));
        }
    }
}
