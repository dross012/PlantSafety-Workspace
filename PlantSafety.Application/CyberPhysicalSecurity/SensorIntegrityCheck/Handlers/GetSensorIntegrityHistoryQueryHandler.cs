// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetSensorIntegrityHistoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Handlers;

/// <summary>Handles <see cref="GetSensorIntegrityHistoryQuery"/>.</summary>
public sealed class GetSensorIntegrityHistoryQueryHandler : IRequestHandler<GetSensorIntegrityHistoryQuery, Result<IReadOnlyList<SensorIntegrityCheckDto>>>
{
    private readonly ISensorIntegrityCheckReadRepository _repository;
    private readonly ILogger<GetSensorIntegrityHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSensorIntegrityHistoryQueryHandler(ISensorIntegrityCheckReadRepository repository, ILogger<GetSensorIntegrityHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<SensorIntegrityCheckDto>>> Handle(GetSensorIntegrityHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHistoryForAssetAsync(request.OtAssetId, request.TenantId, cancellationToken);
            return Result<IReadOnlyList<SensorIntegrityCheckDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SensorIntegrityCheck history for OtAsset {OtAssetId}", request.OtAssetId);

            return Result<IReadOnlyList<SensorIntegrityCheckDto>>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.SensorIntegrityCheck.HistoryRetrievalFailed",
                "Unable to retrieve sensor-integrity check history at this time.",
                Guid.NewGuid()));
        }
    }
}
