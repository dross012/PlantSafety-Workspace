// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           ArchiveRCACommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveRCACommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Handles <see cref="ArchiveRCACommand"/>.</summary>
public sealed class ArchiveRCACommandHandler : IRequestHandler<ArchiveRCACommand, Result<RCAStatusResult>>
{
    private readonly IRCARepository _repository;
    private readonly ILogger<ArchiveRCACommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveRCACommandHandler(IRCARepository repository, ILogger<ArchiveRCACommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RCAStatusResult>> Handle(ArchiveRCACommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RCAId.FromGuid(request.RcaId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RCAStatusResult>.Failure(new ApplicationError(
                    "ContinuousImprovement.RCA.NotFound",
                    "The requested root cause analysis could not be found.",
                    request.RcaId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RCAStatusResult>.Success(new RCAStatusResult(
                aggregate.RcaId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive RCA {RcaId}", request.RcaId);

            return Result<RCAStatusResult>.Failure(new ApplicationError(
                "ContinuousImprovement.RCA.ArchiveFailed",
                "Unable to archive the root cause analysis at this time.",
                Guid.NewGuid()));
        }
    }
}
