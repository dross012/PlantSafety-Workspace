// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           RejectPermitsCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for RejectPermitsCommand. Mirrors ApprovePermitsCommandHandler's
//                  tenant-scoped lookup / NotFound-severity / concurrency-propagation conventions.
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

/// <summary>Handles <see cref="RejectPermitsCommand"/>.</summary>
public sealed class RejectPermitsCommandHandler : IRequestHandler<RejectPermitsCommand, Result<PermitsStatusResult>>
{
    private readonly IPermitsRepository _repository;
    private readonly ILogger<RejectPermitsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RejectPermitsCommandHandler(IPermitsRepository repository, ILogger<RejectPermitsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitsStatusResult>> Handle(RejectPermitsCommand request, CancellationToken cancellationToken)
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

            var rejectedBy = UserId.FromGuid(request.RejectedByUserId);

            aggregate.Reject(
                rejectedByUserId: rejectedBy,
                reason: request.Reason,
                rejectedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(rejectedBy),
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
            _logger.LogError(ex, "Failed to reject Permits {PermitsId}", request.PermitsId);

            return Result<PermitsStatusResult>.Failure(new ApplicationError(
                "Compliance.Permits.RejectFailed",
                "Unable to reject the permit at this time.",
                Guid.NewGuid()));
        }
    }
}
