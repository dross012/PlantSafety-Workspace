// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           CreateFeatureSetCommandHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateFeatureSetCommand. Pure orchestration: constructs the
//                  FeatureSet entity via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Commands
//                  .CreateDataLakeActivityCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Commands;

/// <summary>Handles <see cref="CreateFeatureSetCommand"/>.</summary>
public sealed class CreateFeatureSetCommandHandler : IRequestHandler<CreateFeatureSetCommand, Result<FeatureSetStatusResult>>
{
    private readonly IFeatureSetRepository _repository;
    private readonly ILogger<CreateFeatureSetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateFeatureSetCommandHandler(IFeatureSetRepository repository, ILogger<CreateFeatureSetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureSetStatusResult>> Handle(CreateFeatureSetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = UserId.FromGuid(request.CreatedByUserId);
            var version = string.IsNullOrWhiteSpace(request.Version) ? FeatureVersion.Initial : FeatureVersion.From(request.Version);

            var featureSet = PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet.Create(
                createdBy: createdBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(createdBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                name: request.Name,
                version: version,
                dataSourcePath: request.DataSourcePath);

            await _repository.AddAsync(featureSet, cancellationToken);

            return Result<FeatureSetStatusResult>.Success(new FeatureSetStatusResult(
                featureSet.FeatureSetId.Value,
                featureSet.IsPublished,
                featureSet.Features.Count));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid CreateFeatureSetCommand for Tenant {TenantId}", request.TenantId);

            return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.CreateInvalid",
                ex.Message,
                Guid.NewGuid(),
                ErrorSeverity.Validation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create FeatureSet for Tenant {TenantId}", request.TenantId);

            return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.CreateFailed",
                "Unable to create the feature set at this time.",
                Guid.NewGuid()));
        }
    }
}
