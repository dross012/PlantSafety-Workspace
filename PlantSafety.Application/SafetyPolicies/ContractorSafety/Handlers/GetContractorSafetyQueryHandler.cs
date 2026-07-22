// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetContractorSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.ContractorSafety.Queries;
using PlantSafety.Application.SafetyPolicies.ContractorSafety.Repositories;

namespace PlantSafety.Application.SafetyPolicies.ContractorSafety.Handlers;

/// <summary>Handles <see cref="GetContractorSafetyQuery"/>.</summary>
public sealed class GetContractorSafetyQueryHandler : IRequestHandler<GetContractorSafetyQuery, Result<ContractorSafetyQueryResult>>
{
    private readonly IContractorSafetyRepository _repository;
    private readonly ILogger<GetContractorSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetContractorSafetyQueryHandler(IContractorSafetyRepository repository, ILogger<GetContractorSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContractorSafetyQueryResult>> Handle(GetContractorSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetContractorSafetyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ContractorSafetyQueryResult>.Success(new ContractorSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ContractorSafety for Tenant {TenantId}", request.TenantId);

            return Result<ContractorSafetyQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.ContractorSafety.RetrievalFailed",
                "Unable to retrieve ContractorSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}