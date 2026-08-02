// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           ActivateEnterpriseFabricCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateEnterpriseFabricCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.StartAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnterpriseFabric.Repositories;
using PlantSafety.Domain.EnterpriseFabric.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnterpriseFabric.Commands;

/// <summary>Handles <see cref="ActivateEnterpriseFabricCommand"/>.</summary>
public sealed class ActivateEnterpriseFabricCommandHandler : IRequestHandler<ActivateEnterpriseFabricCommand, Result<EnterpriseFabricStatusResult>>
{
    private readonly IEnterpriseFabricRepository _repository;
    private readonly ILogger<ActivateEnterpriseFabricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateEnterpriseFabricCommandHandler(IEnterpriseFabricRepository repository, ILogger<ActivateEnterpriseFabricCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnterpriseFabricStatusResult>> Handle(ActivateEnterpriseFabricCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EnterpriseFabricId.FromGuid(request.EnterpriseFabricId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EnterpriseFabricStatusResult>.Failure(new ApplicationError(
                    "EnterpriseFabric.NotFound",
                    "The requested enterprise fabric record could not be found.",
                    request.EnterpriseFabricId,
                    ErrorSeverity.NotFound));
            }

            var actorUserId = UserId.FromGuid(request.ActorUserId);

            aggregate.Activate(
                actorUserId: actorUserId,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actorUserId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EnterpriseFabricStatusResult>.Success(new EnterpriseFabricStatusResult(
                aggregate.EnterpriseFabricId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate EnterpriseFabric {EnterpriseFabricId}", request.EnterpriseFabricId);

            return Result<EnterpriseFabricStatusResult>.Failure(new ApplicationError(
                "EnterpriseFabric.ActivateFailed",
                "Unable to activate the enterprise fabric record at this time.",
                Guid.NewGuid()));
        }
    }
}
