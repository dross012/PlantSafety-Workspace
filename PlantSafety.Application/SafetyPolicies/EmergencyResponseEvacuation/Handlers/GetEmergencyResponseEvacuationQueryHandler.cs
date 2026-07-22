// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEmergencyResponseEvacuationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Queries;
using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Repositories;

namespace PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Handlers;

/// <summary>Handles <see cref="GetEmergencyResponseEvacuationQuery"/>.</summary>
public sealed class GetEmergencyResponseEvacuationQueryHandler : IRequestHandler<GetEmergencyResponseEvacuationQuery, Result<EmergencyResponseEvacuationQueryResult>>
{
    private readonly IEmergencyResponseEvacuationRepository _repository;
    private readonly ILogger<GetEmergencyResponseEvacuationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyResponseEvacuationQueryHandler(IEmergencyResponseEvacuationRepository repository, ILogger<GetEmergencyResponseEvacuationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyResponseEvacuationQueryResult>> Handle(GetEmergencyResponseEvacuationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergencyResponseEvacuationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyResponseEvacuationQueryResult>.Success(new EmergencyResponseEvacuationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyResponseEvacuation for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyResponseEvacuationQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.EmergencyResponseEvacuation.RetrievalFailed",
                "Unable to retrieve EmergencyResponseEvacuation data at this time.",
                Guid.NewGuid()));
        }
    }
}