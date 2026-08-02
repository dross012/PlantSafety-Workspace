// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Handler for GetRiskByIdQuery. Reads from the read-side repository (optimised
//   stored proc; no domain-aggregate reconstruction). Returns a NotFound failure both when the row
//   does not exist and when it belongs to a different tenant, so tenant membership is never
//   revealed to an unauthorized caller.
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- tenantId is now pushed into the repository call itself (usp_Risk_GetById's
//   WHERE clause), not just checked here after the fact -- closes the cross-tenant read at the DB
//   boundary. The post-fetch TenantId comparison below is now redundant defense-in-depth (SQL
//   already won't return a cross-tenant row) rather than the primary control.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.Models;
using PlantSafety.Application.RiskManagement.Queries;
using PlantSafety.Application.RiskManagement.Repositories;

namespace PlantSafety.Application.RiskManagement.Handlers;

/// <summary>Handles <see cref="GetRiskByIdQuery"/>.</summary>
public sealed class GetRiskByIdQueryHandler : IRequestHandler<GetRiskByIdQuery, Result<RiskItemDto>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<GetRiskByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRiskByIdQueryHandler(IRiskRepository repository, ILogger<GetRiskByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskItemDto>> Handle(GetRiskByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.RiskId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Risk {RiskId} not found for Tenant {TenantId}", request.RiskId, request.TenantId);

                return Result<RiskItemDto>.Failure(new ApplicationError(
                    "RiskManagement.Risk.NotFound",
                    "The requested risk could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<RiskItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Risk {RiskId}", request.RiskId);

            return Result<RiskItemDto>.Failure(new ApplicationError(
                "RiskManagement.Risk.RetrievalFailed",
                "Unable to retrieve risk data at this time.",
                Guid.NewGuid()));
        }
    }
}
