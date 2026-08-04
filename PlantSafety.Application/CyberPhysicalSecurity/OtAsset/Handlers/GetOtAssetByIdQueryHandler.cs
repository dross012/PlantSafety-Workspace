// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetOtAssetByIdQuery. Reads from the read-side repository.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Handlers;

/// <summary>Handles <see cref="GetOtAssetByIdQuery"/>.</summary>
public sealed class GetOtAssetByIdQueryHandler : IRequestHandler<GetOtAssetByIdQuery, Result<OtAssetDto>>
{
    private readonly IOtAssetReadRepository _repository;
    private readonly ILogger<GetOtAssetByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOtAssetByIdQueryHandler(IOtAssetReadRepository repository, ILogger<GetOtAssetByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OtAssetDto>> Handle(GetOtAssetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.OtAssetId, request.TenantId, cancellationToken);

            if (item is null)
            {
                _logger.LogDebug("OtAsset {OtAssetId} not found for Tenant {TenantId}", request.OtAssetId, request.TenantId);

                return Result<OtAssetDto>.Failure(new ApplicationError(
                    "CyberPhysicalSecurity.OtAsset.NotFound",
                    "The requested OT asset could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<OtAssetDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve OtAsset {OtAssetId}", request.OtAssetId);

            return Result<OtAssetDto>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.OtAsset.RetrievalFailed",
                "Unable to retrieve OT asset data at this time.",
                Guid.NewGuid()));
        }
    }
}
