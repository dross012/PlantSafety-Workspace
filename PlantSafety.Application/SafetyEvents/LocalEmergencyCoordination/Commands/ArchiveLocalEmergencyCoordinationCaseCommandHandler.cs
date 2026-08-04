// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ArchiveLocalEmergencyCoordinationCaseCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveLocalEmergencyCoordinationCaseCommand. Mirrors ArchiveEmergencyCommandHandler.
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

/// <summary>Handles <see cref="ArchiveLocalEmergencyCoordinationCaseCommand"/>.</summary>
public sealed class ArchiveLocalEmergencyCoordinationCaseCommandHandler : IRequestHandler<ArchiveLocalEmergencyCoordinationCaseCommand, Result<LocalEmergencyCoordinationStatusResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<ArchiveLocalEmergencyCoordinationCaseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveLocalEmergencyCoordinationCaseCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<ArchiveLocalEmergencyCoordinationCaseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LocalEmergencyCoordinationStatusResult>> Handle(ArchiveLocalEmergencyCoordinationCaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LocalEmergencyCoordinationId.FromGuid(request.LocalEmergencyCoordinationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<LocalEmergencyCoordinationStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    request.LocalEmergencyCoordinationId,
                    ErrorSeverity.NotFound));
            }

            var archivedBy = UserId.FromGuid(request.ArchivedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(archivedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<LocalEmergencyCoordinationStatusResult>.Success(new LocalEmergencyCoordinationStatusResult(
                aggregate.LocalEmergencyCoordinationId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<LocalEmergencyCoordinationStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.ArchiveFailed",
                "Unable to archive the local emergency coordination case at this time.",
                Guid.NewGuid()));
        }
    }
}
