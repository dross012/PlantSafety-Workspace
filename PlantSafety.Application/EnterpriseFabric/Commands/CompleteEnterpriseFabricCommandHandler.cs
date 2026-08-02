// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           CompleteEnterpriseFabricCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteEnterpriseFabricCommand. Mirrors
//                  ActivateEnterpriseFabricCommandHandler.
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

/// <summary>Handles <see cref="CompleteEnterpriseFabricCommand"/>.</summary>
public sealed class CompleteEnterpriseFabricCommandHandler : IRequestHandler<CompleteEnterpriseFabricCommand, Result<EnterpriseFabricStatusResult>>
{
    private readonly IEnterpriseFabricRepository _repository;
    private readonly ILogger<CompleteEnterpriseFabricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteEnterpriseFabricCommandHandler(IEnterpriseFabricRepository repository, ILogger<CompleteEnterpriseFabricCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnterpriseFabricStatusResult>> Handle(CompleteEnterpriseFabricCommand request, CancellationToken cancellationToken)
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

            aggregate.Complete(
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
            _logger.LogError(ex, "Failed to complete EnterpriseFabric {EnterpriseFabricId}", request.EnterpriseFabricId);

            return Result<EnterpriseFabricStatusResult>.Failure(new ApplicationError(
                "EnterpriseFabric.CompleteFailed",
                "Unable to complete the enterprise fabric record at this time.",
                Guid.NewGuid()));
        }
    }
}
