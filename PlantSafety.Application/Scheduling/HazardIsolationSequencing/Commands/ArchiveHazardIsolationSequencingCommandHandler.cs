// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           ArchiveHazardIsolationSequencingCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveHazardIsolationSequencingCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Commands;

/// <summary>Handles <see cref="ArchiveHazardIsolationSequencingCommand"/>.</summary>
public sealed class ArchiveHazardIsolationSequencingCommandHandler : IRequestHandler<ArchiveHazardIsolationSequencingCommand, Result<HazardIsolationSequencingStatusResult>>
{
    private readonly IHazardIsolationSequencingRepository _repository;
    private readonly ILogger<ArchiveHazardIsolationSequencingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveHazardIsolationSequencingCommandHandler(IHazardIsolationSequencingRepository repository, ILogger<ArchiveHazardIsolationSequencingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardIsolationSequencingStatusResult>> Handle(ArchiveHazardIsolationSequencingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazardIsolationSequencingId.FromGuid(request.HazardIsolationSequencingId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazardIsolationSequencingStatusResult>.Failure(new ApplicationError(
                    "Scheduling.HazardIsolationSequencing.NotFound",
                    "The requested hazard isolation sequencing record could not be found.",
                    request.HazardIsolationSequencingId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<HazardIsolationSequencingStatusResult>.Success(new HazardIsolationSequencingStatusResult(
                aggregate.HazardIsolationSequencingId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive HazardIsolationSequencing {HazardIsolationSequencingId}", request.HazardIsolationSequencingId);

            return Result<HazardIsolationSequencingStatusResult>.Failure(new ApplicationError(
                "Scheduling.HazardIsolationSequencing.ArchiveFailed",
                "Unable to archive the hazard isolation window at this time.",
                Guid.NewGuid()));
        }
    }
}
