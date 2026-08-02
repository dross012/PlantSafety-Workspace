// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           CloseOperationalSafetyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseOperationalSafetyCommand.
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

/// <summary>Handles <see cref="CloseOperationalSafetyCommand"/>.</summary>
public sealed class CloseOperationalSafetyCommandHandler : IRequestHandler<CloseOperationalSafetyCommand, Result<OperationalSafetyStatusResult>>
{
    private readonly IOperationalSafetyRepository _repository;
    private readonly ILogger<CloseOperationalSafetyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseOperationalSafetyCommandHandler(IOperationalSafetyRepository repository, ILogger<CloseOperationalSafetyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperationalSafetyStatusResult>> Handle(CloseOperationalSafetyCommand request, CancellationToken cancellationToken)
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

            aggregate.Close(
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
            _logger.LogError(ex, "Failed to close OperationalSafety {OperationalSafetyId}", request.OperationalSafetyId);

            return Result<OperationalSafetyStatusResult>.Failure(new ApplicationError(
                "OperationalSafety.CloseFailed",
                "Unable to close the operational-safety record at this time.",
                Guid.NewGuid()));
        }
    }
}
