// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           GetDataLakeActivityByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetDataLakeActivityByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  Mirrors PlantSafety.Application.ContinuousImprovement.RCA.Handlers
//                  .GetRCAByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.DataLake.Models;
using PlantSafety.Application.Intelligence.DataLake.Queries;
using PlantSafety.Application.Intelligence.DataLake.Repositories;

namespace PlantSafety.Application.Intelligence.DataLake.Handlers;

/// <summary>Handles <see cref="GetDataLakeActivityByIdQuery"/>.</summary>
public sealed class GetDataLakeActivityByIdQueryHandler : IRequestHandler<GetDataLakeActivityByIdQuery, Result<DataLakeActivityDetailDto>>
{
    private readonly IDataLakeActivityDetailRepository _repository;
    private readonly ILogger<GetDataLakeActivityByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDataLakeActivityByIdQueryHandler(IDataLakeActivityDetailRepository repository, ILogger<GetDataLakeActivityByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DataLakeActivityDetailDto>> Handle(GetDataLakeActivityByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.DataLakeActivityId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("DataLakeActivity {DataLakeActivityId} not found for Tenant {TenantId}", request.DataLakeActivityId, request.TenantId);

                return Result<DataLakeActivityDetailDto>.Failure(new ApplicationError(
                    "Intelligence.DataLake.NotFound",
                    "The requested data lake activity could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<DataLakeActivityDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DataLakeActivity {DataLakeActivityId}", request.DataLakeActivityId);

            return Result<DataLakeActivityDetailDto>.Failure(new ApplicationError(
                "Intelligence.DataLake.RetrievalFailed",
                "Unable to retrieve data lake activity data at this time.",
                Guid.NewGuid()));
        }
    }
}
