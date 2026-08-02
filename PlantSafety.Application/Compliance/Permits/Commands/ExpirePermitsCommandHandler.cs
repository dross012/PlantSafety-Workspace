// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           ExpirePermitsCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ExpirePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Permits.Interfaces;
using PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Handles <see cref="ExpirePermitsCommand"/>.</summary>
public sealed class ExpirePermitsCommandHandler : IRequestHandler<ExpirePermitsCommand, Result<PermitsStatusResult>>
{
    private readonly IPermitsRepository _repository;
    private readonly ILogger<ExpirePermitsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExpirePermitsCommandHandler(IPermitsRepository repository, ILogger<ExpirePermitsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitsStatusResult>> Handle(ExpirePermitsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PermitsId.FromGuid(request.PermitsId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PermitsStatusResult>.Failure(new ApplicationError(
                    "Compliance.Permits.NotFound",
                    "The requested permit could not be found.",
                    request.PermitsId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Expire(
                expiredAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PermitsStatusResult>.Success(new PermitsStatusResult(
                aggregate.PermitsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to expire Permits {PermitsId}", request.PermitsId);

            return Result<PermitsStatusResult>.Failure(new ApplicationError(
                "Compliance.Permits.ExpireFailed",
                "Unable to expire the permit at this time.",
                Guid.NewGuid()));
        }
    }
}
