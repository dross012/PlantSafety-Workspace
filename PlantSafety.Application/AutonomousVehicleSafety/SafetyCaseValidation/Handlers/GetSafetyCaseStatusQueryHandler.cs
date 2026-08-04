// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetSafetyCaseStatusQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Models;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Repositories;

namespace PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Handlers;

/// <summary>Handles <see cref="GetSafetyCaseStatusQuery"/>.</summary>
public sealed class GetSafetyCaseStatusQueryHandler : IRequestHandler<GetSafetyCaseStatusQuery, Result<SafetyCaseValidationItemDto?>>
{
    private readonly ISafetyCaseValidationRepository _repository;
    private readonly ILogger<GetSafetyCaseStatusQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyCaseStatusQueryHandler(ISafetyCaseValidationRepository repository, ILogger<GetSafetyCaseStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyCaseValidationItemDto?>> Handle(GetSafetyCaseStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetSafetyCaseStatusByAssetIdAsync(request.TenantId, request.AutonomousVehicleAssetId, cancellationToken);
            return Result<SafetyCaseValidationItemDto?>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve safety-case status for Asset {AutonomousVehicleAssetId}, Tenant {TenantId}", request.AutonomousVehicleAssetId, request.TenantId);

            return Result<SafetyCaseValidationItemDto?>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.SafetyCaseValidation.RetrievalFailed",
                "Unable to retrieve the safety-case status at this time.",
                Guid.NewGuid()));
        }
    }
}
