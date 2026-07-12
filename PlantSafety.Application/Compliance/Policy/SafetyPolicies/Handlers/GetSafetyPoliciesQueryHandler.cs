// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSafetyPoliciesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Queries;
using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Repositories;

namespace PlantSafety.Application.Compliance.Policy.SafetyPolicies.Handlers;

/// <summary>Handles <see cref="GetSafetyPoliciesQuery"/>.</summary>
public sealed class GetSafetyPoliciesQueryHandler : IRequestHandler<GetSafetyPoliciesQuery, Result<SafetyPoliciesQueryResult>>
{
    private readonly ISafetyPoliciesRepository _repository;
    private readonly ILogger<GetSafetyPoliciesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyPoliciesQueryHandler(ISafetyPoliciesRepository repository, ILogger<GetSafetyPoliciesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyPoliciesQueryResult>> Handle(GetSafetyPoliciesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSafetyPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetyPoliciesQueryResult>.Success(new SafetyPoliciesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyPolicies for Tenant {TenantId}", request.TenantId);

            return Result<SafetyPoliciesQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.SafetyPolicies.RetrievalFailed",
                "Unable to retrieve SafetyPolicies data at this time.",
                Guid.NewGuid()));
        }
    }
}
