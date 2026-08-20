// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           GetModelRegistryEntryByIdQueryHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for GetModelRegistryEntryByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  Mirrors PlantSafety.Application.Intelligence.DataLake.Handlers
//                  .GetDataLakeActivityByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.ModelRegistry.Models;
using PlantSafety.Application.Intelligence.ModelRegistry.Queries;
using PlantSafety.Application.Intelligence.ModelRegistry.Repositories;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Handlers;

/// <summary>Handles <see cref="GetModelRegistryEntryByIdQuery"/>.</summary>
public sealed class GetModelRegistryEntryByIdQueryHandler : IRequestHandler<GetModelRegistryEntryByIdQuery, Result<ModelRegistryEntryDetailDto>>
{
    private readonly IModelRegistryEntryDetailRepository _repository;
    private readonly ILogger<GetModelRegistryEntryByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetModelRegistryEntryByIdQueryHandler(IModelRegistryEntryDetailRepository repository, ILogger<GetModelRegistryEntryByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ModelRegistryEntryDetailDto>> Handle(GetModelRegistryEntryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ModelRegistryEntryId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("ModelRegistryEntry {ModelRegistryEntryId} not found for Tenant {TenantId}", request.ModelRegistryEntryId, request.TenantId);

                return Result<ModelRegistryEntryDetailDto>.Failure(new ApplicationError(
                    "Intelligence.ModelRegistry.NotFound",
                    "The requested model registry entry could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ModelRegistryEntryDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ModelRegistryEntry {ModelRegistryEntryId}", request.ModelRegistryEntryId);

            return Result<ModelRegistryEntryDetailDto>.Failure(new ApplicationError(
                "Intelligence.ModelRegistry.RetrievalFailed",
                "Unable to retrieve model registry data at this time.",
                Guid.NewGuid()));
        }
    }
}
