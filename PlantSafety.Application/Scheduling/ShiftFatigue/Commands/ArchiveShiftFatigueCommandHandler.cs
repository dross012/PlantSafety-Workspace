// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           ArchiveShiftFatigueCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveShiftFatigueCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Interfaces;
using PlantSafety.Domain.Scheduling.ShiftFatigue.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Commands;

/// <summary>Handles <see cref="ArchiveShiftFatigueCommand"/>.</summary>
public sealed class ArchiveShiftFatigueCommandHandler : IRequestHandler<ArchiveShiftFatigueCommand, Result<ShiftFatigueStatusResult>>
{
    private readonly IShiftFatigueRepository _repository;
    private readonly ILogger<ArchiveShiftFatigueCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveShiftFatigueCommandHandler(IShiftFatigueRepository repository, ILogger<ArchiveShiftFatigueCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ShiftFatigueStatusResult>> Handle(ArchiveShiftFatigueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ShiftFatigueId.FromGuid(request.ShiftFatigueId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ShiftFatigueStatusResult>.Failure(new ApplicationError(
                    "Scheduling.ShiftFatigue.NotFound",
                    "The requested shift fatigue record could not be found.",
                    request.ShiftFatigueId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ShiftFatigueStatusResult>.Success(new ShiftFatigueStatusResult(
                aggregate.ShiftFatigueId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive ShiftFatigue {ShiftFatigueId}", request.ShiftFatigueId);

            return Result<ShiftFatigueStatusResult>.Failure(new ApplicationError(
                "Scheduling.ShiftFatigue.ArchiveFailed",
                "Unable to archive the fatigue record at this time.",
                Guid.NewGuid()));
        }
    }
}
