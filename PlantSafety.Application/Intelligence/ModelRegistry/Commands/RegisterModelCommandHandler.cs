// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           RegisterModelCommandHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for RegisterModelCommand. Pure orchestration: constructs the
//                  ModelRegistryEntry aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Commands
//                  .CreateDataLakeActivityCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Commands;

/// <summary>Handles <see cref="RegisterModelCommand"/>.</summary>
public sealed class RegisterModelCommandHandler : IRequestHandler<RegisterModelCommand, Result<ModelRegistryEntryStatusResult>>
{
    private readonly IModelRegistryEntryRepository _repository;
    private readonly ILogger<RegisterModelCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterModelCommandHandler(IModelRegistryEntryRepository repository, ILogger<RegisterModelCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ModelRegistryEntryStatusResult>> Handle(RegisterModelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var registeredBy = UserId.FromGuid(request.RegisteredByUserId);
            var version = ModelVersion.From(request.ModelVersion);

            var aggregate = PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry.Register(
                registeredBy: registeredBy,
                registeredAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(registeredBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                modelName: request.ModelName,
                version: version,
                datasetHash: request.DatasetHash,
                trainingAccuracyScore: request.TrainingAccuracyScore);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<ModelRegistryEntryStatusResult>.Success(new ModelRegistryEntryStatusResult(
                aggregate.ModelRegistryEntryId.Value,
                aggregate.Status.ToString()));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RegisterModelCommand for Tenant {TenantId}", request.TenantId);

            return Result<ModelRegistryEntryStatusResult>.Failure(new ApplicationError(
                "Intelligence.ModelRegistry.RegisterInvalid",
                ex.Message,
                Guid.NewGuid(),
                ErrorSeverity.Validation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register model for Tenant {TenantId}", request.TenantId);

            return Result<ModelRegistryEntryStatusResult>.Failure(new ApplicationError(
                "Intelligence.ModelRegistry.RegisterFailed",
                "Unable to register the model at this time.",
                Guid.NewGuid()));
        }
    }
}
