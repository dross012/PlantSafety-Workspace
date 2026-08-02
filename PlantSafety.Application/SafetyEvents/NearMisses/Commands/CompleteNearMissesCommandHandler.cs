// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           CompleteNearMissesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteNearMissesCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
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

/// <summary>Handles <see cref="CompleteNearMissesCommand"/>.</summary>
public sealed class CompleteNearMissesCommandHandler : IRequestHandler<CompleteNearMissesCommand, Result<NearMissesStatusResult>>
{
    private readonly INearMissesRepository _repository;
    private readonly ILogger<CompleteNearMissesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteNearMissesCommandHandler(INearMissesRepository repository, ILogger<CompleteNearMissesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissesStatusResult>> Handle(CompleteNearMissesCommand request, CancellationToken cancellationToken)
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

            var completedBy = UserId.FromGuid(request.CompletedByUserId);

            aggregate.Complete(
                completedBy: completedBy,
                findings: request.Findings,
                completedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(completedBy),
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
            _logger.LogError(ex, "Failed to complete NearMisses {NearMissesId}", request.NearMissesId);

            return Result<NearMissesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.NearMisses.CompleteFailed",
                "Unable to complete the near miss review at this time.",
                Guid.NewGuid()));
        }
    }
}
