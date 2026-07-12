// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEmergencyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Emergency.Queries;
using PlantSafety.Application.Compliance.Policy.Emergency.Repositories;

namespace PlantSafety.Application.Compliance.Policy.Emergency.Handlers;

/// <summary>Handles <see cref="GetEmergencyQuery"/>.</summary>
public sealed class GetEmergencyQueryHandler : IRequestHandler<GetEmergencyQuery, Result<EmergencyQueryResult>>
{
    private readonly IEmergencyRepository _repository;
    private readonly ILogger<GetEmergencyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyQueryHandler(IEmergencyRepository repository, ILogger<GetEmergencyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyQueryResult>> Handle(GetEmergencyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergencyPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyQueryResult>.Success(new EmergencyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Emergency for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.Emergency.RetrievalFailed",
                "Unable to retrieve Emergency data at this time.",
                Guid.NewGuid()));
        }
    }
}
