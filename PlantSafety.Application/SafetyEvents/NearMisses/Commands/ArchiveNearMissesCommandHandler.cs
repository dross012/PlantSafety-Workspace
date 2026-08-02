// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           ArchiveNearMissesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveNearMissesCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces;
using PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Commands;

/// <summary>Handles <see cref="ArchiveNearMissesCommand"/>.</summary>
public sealed class ArchiveNearMissesCommandHandler : IRequestHandler<ArchiveNearMissesCommand, Result<NearMissesStatusResult>>
{
    private readonly INearMissesRepository _repository;
    private readonly ILogger<ArchiveNearMissesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveNearMissesCommandHandler(INearMissesRepository repository, ILogger<ArchiveNearMissesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissesStatusResult>> Handle(ArchiveNearMissesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = NearMissesId.FromGuid(request.NearMissesId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<NearMissesStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.NearMisses.NotFound",
                    "The requested near miss could not be found.",
                    request.NearMissesId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<NearMissesStatusResult>.Success(new NearMissesStatusResult(
                aggregate.NearMissesId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive NearMisses {NearMissesId}", request.NearMissesId);

            return Result<NearMissesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.NearMisses.ArchiveFailed",
                "Unable to archive the near miss at this time.",
                Guid.NewGuid()));
        }
    }
}
