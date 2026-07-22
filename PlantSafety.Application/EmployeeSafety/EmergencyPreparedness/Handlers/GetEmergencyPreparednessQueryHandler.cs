// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEmergencyPreparednessQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Queries;
using PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Repositories;

namespace PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Handlers;

/// <summary>Handles <see cref="GetEmergencyPreparednessQuery"/>.</summary>
public sealed class GetEmergencyPreparednessQueryHandler : IRequestHandler<GetEmergencyPreparednessQuery, Result<EmergencyPreparednessQueryResult>>
{
    private readonly IEmergencyPreparednessRepository _repository;
    private readonly ILogger<GetEmergencyPreparednessQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyPreparednessQueryHandler(IEmergencyPreparednessRepository repository, ILogger<GetEmergencyPreparednessQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyPreparednessQueryResult>> Handle(GetEmergencyPreparednessQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergencyPreparednessItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyPreparednessQueryResult>.Success(new EmergencyPreparednessQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyPreparedness records for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyPreparednessQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.EmergencyPreparedness.RetrievalFailed",
                "Unable to retrieve EmergencyPreparedness records at this time.",
                Guid.NewGuid()));
        }
    }
}