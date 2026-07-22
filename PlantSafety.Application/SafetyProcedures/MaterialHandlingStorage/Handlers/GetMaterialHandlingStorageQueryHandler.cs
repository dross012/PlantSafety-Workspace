// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMaterialHandlingStorageQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Queries;
using PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Repositories;

namespace PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Handlers;

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
            var items = await _repository.GetMaterialHandlingStorageItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MaterialHandlingStorageQueryResult>.Success(new MaterialHandlingStorageQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MaterialHandlingStorage records for Tenant {TenantId}", request.TenantId);

            return Result<MaterialHandlingStorageQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.MaterialHandlingStorage.RetrievalFailed",
                "Unable to retrieve MaterialHandlingStorage records at this time.",
                Guid.NewGuid()));
        }
    }
}