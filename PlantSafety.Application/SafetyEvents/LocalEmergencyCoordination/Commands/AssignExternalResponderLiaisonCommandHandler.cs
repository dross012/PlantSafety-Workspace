// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           AssignExternalResponderLiaisonCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for AssignExternalResponderLiaisonCommand. Mirrors RespondToEmergencyCommandHandler.
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

/// <summary>Handles <see cref="AssignExternalResponderLiaisonCommand"/>.</summary>
public sealed class AssignExternalResponderLiaisonCommandHandler : IRequestHandler<AssignExternalResponderLiaisonCommand, Result<AssignExternalResponderLiaisonResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<AssignExternalResponderLiaisonCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AssignExternalResponderLiaisonCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<AssignExternalResponderLiaisonCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AssignExternalResponderLiaisonResult>> Handle(AssignExternalResponderLiaisonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LocalEmergencyCoordinationId.FromGuid(request.LocalEmergencyCoordinationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AssignExternalResponderLiaisonResult>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    request.LocalEmergencyCoordinationId,
                    ErrorSeverity.NotFound));
            }

            var assignedBy = UserId.FromGuid(request.AssignedByUserId);
            var liaisonUserId = UserId.FromGuid(request.LiaisonUserId);

            var liaison = aggregate.AssignExternalResponderLiaison(
                incidentId: request.IncidentId,
                agencyName: request.AgencyName,
                liaisonUserId: liaisonUserId,
                assignedBy: assignedBy,
                assignedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(assignedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AssignExternalResponderLiaisonResult>.Success(new AssignExternalResponderLiaisonResult(
                aggregate.LocalEmergencyCoordinationId.Value, liaison.ExternalResponderLiaisonId.Value));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign external responder liaison for LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<AssignExternalResponderLiaisonResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.AssignExternalResponderLiaisonFailed",
                "Unable to assign the external responder liaison at this time.",
                Guid.NewGuid()));
        }
    }
}
