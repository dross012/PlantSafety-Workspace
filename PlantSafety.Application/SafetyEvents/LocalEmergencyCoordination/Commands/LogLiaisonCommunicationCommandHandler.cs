// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           LogLiaisonCommunicationCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for LogLiaisonCommunicationCommand. Mirrors RespondToEmergencyCommandHandler.
//                  Liaison logs are retained with the incident record for the full incident
//                  retention period per the module's data governance requirement (enforced at the
//                  Infrastructure retention-policy layer, not here).
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

/// <summary>Handles <see cref="LogLiaisonCommunicationCommand"/>.</summary>
public sealed class LogLiaisonCommunicationCommandHandler : IRequestHandler<LogLiaisonCommunicationCommand, Result<LogLiaisonCommunicationResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<LogLiaisonCommunicationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public LogLiaisonCommunicationCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<LogLiaisonCommunicationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LogLiaisonCommunicationResult>> Handle(LogLiaisonCommunicationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LocalEmergencyCoordinationId.FromGuid(request.LocalEmergencyCoordinationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<LogLiaisonCommunicationResult>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    request.LocalEmergencyCoordinationId,
                    ErrorSeverity.NotFound));
            }

            var loggedBy = UserId.FromGuid(request.LoggedByUserId);
            var liaisonId = ExternalResponderLiaisonId.FromGuid(request.ExternalResponderLiaisonId);

            var entry = aggregate.LogLiaisonCommunication(
                externalResponderLiaisonId: liaisonId,
                message: request.Message,
                loggedBy: loggedBy,
                loggedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(loggedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<LogLiaisonCommunicationResult>.Success(new LogLiaisonCommunicationResult(
                aggregate.LocalEmergencyCoordinationId.Value, request.ExternalResponderLiaisonId, entry.LiaisonLogEntryId.Value));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log liaison communication for LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<LogLiaisonCommunicationResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.LogLiaisonCommunicationFailed",
                "Unable to log the liaison communication at this time.",
                Guid.NewGuid()));
        }
    }
}
