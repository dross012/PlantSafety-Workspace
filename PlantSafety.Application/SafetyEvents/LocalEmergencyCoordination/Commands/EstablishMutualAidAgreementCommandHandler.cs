// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           EstablishMutualAidAgreementCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for EstablishMutualAidAgreementCommand. Mirrors RespondToEmergencyCommandHandler.
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

/// <summary>Handles <see cref="EstablishMutualAidAgreementCommand"/>.</summary>
public sealed class EstablishMutualAidAgreementCommandHandler : IRequestHandler<EstablishMutualAidAgreementCommand, Result<EstablishMutualAidAgreementResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<EstablishMutualAidAgreementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EstablishMutualAidAgreementCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<EstablishMutualAidAgreementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EstablishMutualAidAgreementResult>> Handle(EstablishMutualAidAgreementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LocalEmergencyCoordinationId.FromGuid(request.LocalEmergencyCoordinationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EstablishMutualAidAgreementResult>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    request.LocalEmergencyCoordinationId,
                    ErrorSeverity.NotFound));
            }

            var establishedBy = UserId.FromGuid(request.EstablishedByUserId);
            var now = UtcTimestamp.Now();

            var agreement = aggregate.EstablishMutualAidAgreement(
                agencyName: request.AgencyName,
                discipline: request.Discipline,
                documentVersion: request.DocumentVersion,
                effectiveFromUtc: UtcTimestamp.From(request.EffectiveFromUtc),
                renewalDueAtUtc: UtcTimestamp.From(request.RenewalDueAtUtc),
                establishedBy: establishedBy,
                establishedAtUtc: now,
                actor: PerformedByActor.User(establishedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EstablishMutualAidAgreementResult>.Success(new EstablishMutualAidAgreementResult(
                aggregate.LocalEmergencyCoordinationId.Value,
                agreement.MutualAidAgreementId.Value,
                agreement.RenewalStatus.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish mutual aid agreement for LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<EstablishMutualAidAgreementResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.EstablishMutualAidAgreementFailed",
                "Unable to establish the mutual aid agreement at this time.",
                Guid.NewGuid()));
        }
    }
}
