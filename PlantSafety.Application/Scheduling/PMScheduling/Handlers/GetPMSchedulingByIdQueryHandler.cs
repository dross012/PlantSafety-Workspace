// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-30
// FILE DESCRIPTION: Handler for GetPMSchedulingByIdQuery. Reads from the read-side repository
//   (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound failure both
//   when the row does not exist and when it belongs to a different tenant, so tenant membership is
//   never revealed to an unauthorized caller. Mirrors
//   PlantSafety.Application.RiskManagement.Handlers.GetRiskByIdQueryHandler.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PMScheduling.Models;
using PlantSafety.Application.Scheduling.PMScheduling.Queries;
using PlantSafety.Application.Scheduling.PMScheduling.Repositories;

namespace PlantSafety.Application.Scheduling.PMScheduling.Handlers;

/// <summary>Handles <see cref="GetPMSchedulingByIdQuery"/>.</summary>
public sealed class GetPMSchedulingByIdQueryHandler : IRequestHandler<GetPMSchedulingByIdQuery, Result<PMSchedulingItemDto>>
{
    private readonly IPMSchedulingRepository _repository;
    private readonly ILogger<GetPMSchedulingByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPMSchedulingByIdQueryHandler(IPMSchedulingRepository repository, ILogger<GetPMSchedulingByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PMSchedulingItemDto>> Handle(GetPMSchedulingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PMSchedulingId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("PMScheduling {PMSchedulingId} not found for Tenant {TenantId}", request.PMSchedulingId, request.TenantId);

                return Result<PMSchedulingItemDto>.Failure(new ApplicationError(
                    "Scheduling.PMScheduling.NotFound",
                    "The requested PM scheduling record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PMSchedulingItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PMScheduling {PMSchedulingId}", request.PMSchedulingId);

            return Result<PMSchedulingItemDto>.Failure(new ApplicationError(
                "Scheduling.PMScheduling.RetrievalFailed",
                "Unable to retrieve PM scheduling data at this time.",
                Guid.NewGuid()));
        }
    }
}
