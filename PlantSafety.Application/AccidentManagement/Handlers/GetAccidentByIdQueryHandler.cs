// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Handler for GetAccidentByIdQuery. Reads from the read-side repository
//   (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound failure both
//   when the row does not exist and when it belongs to a different tenant, so tenant membership is
//   never revealed to an unauthorized caller. tenantId is pushed into the repository call itself
//   (usp_Accident_GetById's WHERE clause), not just checked here after the fact -- the post-fetch
//   TenantId comparison below is defense-in-depth, not the primary control. Mirrors
//   PlantSafety.Application.RiskManagement.Handlers.GetRiskByIdQueryHandler.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.AccidentManagement.Models;
using PlantSafety.Application.AccidentManagement.Queries;
using PlantSafety.Application.AccidentManagement.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.Handlers;

/// <summary>Handles <see cref="GetAccidentByIdQuery"/>.</summary>
public sealed class GetAccidentByIdQueryHandler : IRequestHandler<GetAccidentByIdQuery, Result<AccidentItemDto>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<GetAccidentByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAccidentByIdQueryHandler(IAccidentRepository repository, ILogger<GetAccidentByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccidentItemDto>> Handle(GetAccidentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.AccidentId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Accident {AccidentId} not found for Tenant {TenantId}", request.AccidentId, request.TenantId);

                return Result<AccidentItemDto>.Failure(new ApplicationError(
                    "AccidentManagement.Accident.NotFound",
                    "The requested accident could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<AccidentItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Accident {AccidentId}", request.AccidentId);

            return Result<AccidentItemDto>.Failure(new ApplicationError(
                "AccidentManagement.Accident.RetrievalFailed",
                "Unable to retrieve accident data at this time.",
                Guid.NewGuid()));
        }
    }
}
