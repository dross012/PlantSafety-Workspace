// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetDischargePermitByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Handlers;

/// <summary>Handles <see cref="GetDischargePermitByIdQuery"/>.</summary>
public sealed class GetDischargePermitByIdQueryHandler : IRequestHandler<GetDischargePermitByIdQuery, Result<GetDischargePermitByIdQueryResult>>
{
    private readonly IDischargePermitRepository _repository;
    private readonly ILogger<GetDischargePermitByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDischargePermitByIdQueryHandler(IDischargePermitRepository repository, ILogger<GetDischargePermitByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetDischargePermitByIdQueryResult>> Handle(GetDischargePermitByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDischargePermitByIdAsync(request.TenantId, request.DischargePermitId, cancellationToken);

            return Result<GetDischargePermitByIdQueryResult>.Success(new GetDischargePermitByIdQueryResult(items, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GetDischargePermitByIdQuery");

            return Result<GetDischargePermitByIdQueryResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.GetDischargePermitByIdFailed",
                "Unable to retrieve DischargePermit records at this time.",
                Guid.NewGuid()));
        }
    }
}