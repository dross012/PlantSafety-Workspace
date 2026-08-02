// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           CompleteOperationalSafetyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteOperationalSafetyCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.OperationalSafety.Repositories;
using PlantSafety.Domain.OperationalSafety.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.OperationalSafety.Commands;

/// <summary>Handles <see cref="CompleteOperationalSafetyCommand"/>.</summary>
public sealed class CompleteOperationalSafetyCommandHandler : IRequestHandler<CompleteOperationalSafetyCommand, Result<OperationalSafetyStatusResult>>
{
    private readonly IOperationalSafetyRepository _repository;
    private readonly ILogger<CompleteOperationalSafetyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteOperationalSafetyCommandHandler(IOperationalSafetyRepository repository, ILogger<CompleteOperationalSafetyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperationalSafetyStatusResult>> Handle(CompleteOperationalSafetyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = OperationalSafetyId.FromGuid(request.OperationalSafetyId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<OperationalSafetyStatusResult>.Failure(new ApplicationError(
                    "OperationalSafety.NotFound",
                    "The requested operational-safety record could not be found.",
                    request.OperationalSafetyId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Complete(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<OperationalSafetyStatusResult>.Success(new OperationalSafetyStatusResult(
                aggregate.OperationalSafetyId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete OperationalSafety {OperationalSafetyId}", request.OperationalSafetyId);

            return Result<OperationalSafetyStatusResult>.Failure(new ApplicationError(
                "OperationalSafety.CompleteFailed",
                "Unable to complete the operational-safety record at this time.",
                Guid.NewGuid()));
        }
    }
}
