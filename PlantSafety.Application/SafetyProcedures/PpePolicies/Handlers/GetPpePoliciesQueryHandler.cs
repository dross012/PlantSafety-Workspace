// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPpePoliciesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.PpePolicies.Queries;
using PlantSafety.Application.SafetyProcedures.PpePolicies.Repositories;

namespace PlantSafety.Application.SafetyProcedures.PpePolicies.Handlers;

/// <summary>Handles <see cref="GetPpePoliciesQuery"/>.</summary>
public sealed class GetPpePoliciesQueryHandler : IRequestHandler<GetPpePoliciesQuery, Result<PpePoliciesQueryResult>>
{
    private readonly IPpePoliciesRepository _repository;
    private readonly ILogger<GetPpePoliciesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPpePoliciesQueryHandler(IPpePoliciesRepository repository, ILogger<GetPpePoliciesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpePoliciesQueryResult>> Handle(GetPpePoliciesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPpePoliciesItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PpePoliciesQueryResult>.Success(new PpePoliciesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PpePolicies records for Tenant {TenantId}", request.TenantId);

            return Result<PpePoliciesQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.PpePolicies.RetrievalFailed",
                "Unable to retrieve PpePolicies records at this time.",
                Guid.NewGuid()));
        }
    }
}