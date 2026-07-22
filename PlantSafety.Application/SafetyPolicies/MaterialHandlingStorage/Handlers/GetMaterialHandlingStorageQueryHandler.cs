// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMaterialHandlingStorageQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Queries;
using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Repositories;

namespace PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Handlers;

/// <summary>Handles <see cref="GetMaterialHandlingStorageQuery"/>.</summary>
public sealed class GetMaterialHandlingStorageQueryHandler : IRequestHandler<GetMaterialHandlingStorageQuery, Result<MaterialHandlingStorageQueryResult>>
{
    private readonly IMaterialHandlingStorageRepository _repository;
    private readonly ILogger<GetMaterialHandlingStorageQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMaterialHandlingStorageQueryHandler(IMaterialHandlingStorageRepository repository, ILogger<GetMaterialHandlingStorageQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MaterialHandlingStorageQueryResult>> Handle(GetMaterialHandlingStorageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMaterialHandlingStorageAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MaterialHandlingStorageQueryResult>.Success(new MaterialHandlingStorageQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MaterialHandlingStorage for Tenant {TenantId}", request.TenantId);

            return Result<MaterialHandlingStorageQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.MaterialHandlingStorage.RetrievalFailed",
                "Unable to retrieve MaterialHandlingStorage data at this time.",
                Guid.NewGuid()));
        }
    }
}