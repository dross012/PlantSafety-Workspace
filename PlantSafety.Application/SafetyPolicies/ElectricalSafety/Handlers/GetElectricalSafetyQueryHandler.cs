// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetElectricalSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Queries;
using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Repositories;

namespace PlantSafety.Application.SafetyPolicies.ElectricalSafety.Handlers;

/// <summary>Handles <see cref="GetElectricalSafetyQuery"/>.</summary>
public sealed class GetElectricalSafetyQueryHandler : IRequestHandler<GetElectricalSafetyQuery, Result<ElectricalSafetyQueryResult>>
{
    private readonly IElectricalSafetyRepository _repository;
    private readonly ILogger<GetElectricalSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetElectricalSafetyQueryHandler(IElectricalSafetyRepository repository, ILogger<GetElectricalSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ElectricalSafetyQueryResult>> Handle(GetElectricalSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetElectricalSafetyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ElectricalSafetyQueryResult>.Success(new ElectricalSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ElectricalSafety for Tenant {TenantId}", request.TenantId);

            return Result<ElectricalSafetyQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.ElectricalSafety.RetrievalFailed",
                "Unable to retrieve ElectricalSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}