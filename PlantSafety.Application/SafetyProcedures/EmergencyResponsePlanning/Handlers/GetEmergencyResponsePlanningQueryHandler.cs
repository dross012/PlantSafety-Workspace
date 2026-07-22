// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEmergencyResponsePlanningQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Queries;
using PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Repositories;

namespace PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Handlers;

/// <summary>Handles <see cref="GetEmergencyResponsePlanningQuery"/>.</summary>
public sealed class GetEmergencyResponsePlanningQueryHandler : IRequestHandler<GetEmergencyResponsePlanningQuery, Result<EmergencyResponsePlanningQueryResult>>
{
    private readonly IEmergencyResponsePlanningRepository _repository;
    private readonly ILogger<GetEmergencyResponsePlanningQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyResponsePlanningQueryHandler(IEmergencyResponsePlanningRepository repository, ILogger<GetEmergencyResponsePlanningQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyResponsePlanningQueryResult>> Handle(GetEmergencyResponsePlanningQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergencyResponsePlanningItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyResponsePlanningQueryResult>.Success(new EmergencyResponsePlanningQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyResponsePlanning records for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyResponsePlanningQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.EmergencyResponsePlanning.RetrievalFailed",
                "Unable to retrieve EmergencyResponsePlanning records at this time.",
                Guid.NewGuid()));
        }
    }
}