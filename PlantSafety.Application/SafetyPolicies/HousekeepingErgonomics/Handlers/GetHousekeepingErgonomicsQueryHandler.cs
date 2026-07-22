// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHousekeepingErgonomicsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Queries;
using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Repositories;

namespace PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Handlers;

/// <summary>Handles <see cref="GetHousekeepingErgonomicsQuery"/>.</summary>
public sealed class GetHousekeepingErgonomicsQueryHandler : IRequestHandler<GetHousekeepingErgonomicsQuery, Result<HousekeepingErgonomicsQueryResult>>
{
    private readonly IHousekeepingErgonomicsRepository _repository;
    private readonly ILogger<GetHousekeepingErgonomicsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHousekeepingErgonomicsQueryHandler(IHousekeepingErgonomicsRepository repository, ILogger<GetHousekeepingErgonomicsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HousekeepingErgonomicsQueryResult>> Handle(GetHousekeepingErgonomicsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHousekeepingErgonomicsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HousekeepingErgonomicsQueryResult>.Success(new HousekeepingErgonomicsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HousekeepingErgonomics for Tenant {TenantId}", request.TenantId);

            return Result<HousekeepingErgonomicsQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.HousekeepingErgonomics.RetrievalFailed",
                "Unable to retrieve HousekeepingErgonomics data at this time.",
                Guid.NewGuid()));
        }
    }
}