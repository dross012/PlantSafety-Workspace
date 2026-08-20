// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           DeprecateModelCommandHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for DeprecateModelCommand. GetByIdAsync is tenant-scoped, the NotFound
//                  ApplicationError uses ErrorSeverity.NotFound, and ConcurrencyConflictException is
//                  re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Commands;

/// <summary>Handles <see cref="DeprecateModelCommand"/>.</summary>
public sealed class DeprecateModelCommandHandler : IRequestHandler<DeprecateModelCommand, Result<ModelRegistryEntryStatusResult>>
{
    private readonly IModelRegistryEntryRepository _repository;
    private readonly ILogger<DeprecateModelCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeprecateModelCommandHandler(IModelRegistryEntryRepository repository, ILogger<DeprecateModelCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ModelRegistryEntryStatusResult>> Handle(DeprecateModelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ModelRegistryEntryId.FromGuid(request.ModelRegistryEntryId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ModelRegistryEntryStatusResult>.Failure(new ApplicationError(
                    "Intelligence.ModelRegistry.NotFound",
                    "The requested model registry entry could not be found.",
                    request.ModelRegistryEntryId,
                    ErrorSeverity.NotFound));
            }

            var deprecatedBy = UserId.FromGuid(request.DeprecatedByUserId);

            aggregate.Deprecate(
                reason: request.Reason,
                deprecatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(deprecatedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ModelRegistryEntryStatusResult>.Success(new ModelRegistryEntryStatusResult(
                aggregate.ModelRegistryEntryId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deprecate ModelRegistryEntry {ModelRegistryEntryId}", request.ModelRegistryEntryId);

            return Result<ModelRegistryEntryStatusResult>.Failure(new ApplicationError(
                "Intelligence.ModelRegistry.DeprecateFailed",
                "Unable to deprecate the model at this time.",
                Guid.NewGuid()));
        }
    }
}
