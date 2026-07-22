// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHazardIdentificationMeasurementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Queries;
using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Repositories;

namespace PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Handlers;

/// <summary>Handles <see cref="GetHazardIdentificationMeasurementQuery"/>.</summary>
public sealed class GetHazardIdentificationMeasurementQueryHandler : IRequestHandler<GetHazardIdentificationMeasurementQuery, Result<HazardIdentificationMeasurementQueryResult>>
{
    private readonly IHazardIdentificationMeasurementRepository _repository;
    private readonly ILogger<GetHazardIdentificationMeasurementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardIdentificationMeasurementQueryHandler(IHazardIdentificationMeasurementRepository repository, ILogger<GetHazardIdentificationMeasurementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardIdentificationMeasurementQueryResult>> Handle(GetHazardIdentificationMeasurementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardIdentificationMeasurementAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardIdentificationMeasurementQueryResult>.Success(new HazardIdentificationMeasurementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardIdentificationMeasurement for Tenant {TenantId}", request.TenantId);

            return Result<HazardIdentificationMeasurementQueryResult>.Failure(new ApplicationError(
                "RiskManagement.HazardIdentificationMeasurement.RetrievalFailed",
                "Unable to retrieve HazardIdentificationMeasurement data at this time.",
                Guid.NewGuid()));
        }
    }
}