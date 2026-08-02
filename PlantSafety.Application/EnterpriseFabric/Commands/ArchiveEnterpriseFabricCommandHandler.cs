// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           ArchiveEnterpriseFabricCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveEnterpriseFabricCommand. Mirrors
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

/// <summary>Handles <see cref="ArchiveEnterpriseFabricCommand"/>.</summary>
public sealed class ArchiveEnterpriseFabricCommandHandler : IRequestHandler<ArchiveEnterpriseFabricCommand, Result<EnterpriseFabricStatusResult>>
{
    private readonly IEnterpriseFabricRepository _repository;
    private readonly ILogger<ArchiveEnterpriseFabricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveEnterpriseFabricCommandHandler(IEnterpriseFabricRepository repository, ILogger<ArchiveEnterpriseFabricCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnterpriseFabricStatusResult>> Handle(ArchiveEnterpriseFabricCommand request, CancellationToken cancellationToken)
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

            aggregate.Archive(
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
            _logger.LogError(ex, "Failed to archive EnterpriseFabric {EnterpriseFabricId}", request.EnterpriseFabricId);

            return Result<EnterpriseFabricStatusResult>.Failure(new ApplicationError(
                "EnterpriseFabric.ArchiveFailed",
                "Unable to archive the enterprise fabric record at this time.",
                Guid.NewGuid()));
        }
    }
}
