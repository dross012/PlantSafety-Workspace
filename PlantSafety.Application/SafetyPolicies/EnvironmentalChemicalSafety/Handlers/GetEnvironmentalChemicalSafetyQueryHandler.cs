// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEnvironmentalChemicalSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Queries;
using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Repositories;

namespace PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalChemicalSafetyQuery"/>.</summary>
public sealed class GetEnvironmentalChemicalSafetyQueryHandler : IRequestHandler<GetEnvironmentalChemicalSafetyQuery, Result<EnvironmentalChemicalSafetyQueryResult>>
{
    private readonly IEnvironmentalChemicalSafetyRepository _repository;
    private readonly ILogger<GetEnvironmentalChemicalSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalChemicalSafetyQueryHandler(IEnvironmentalChemicalSafetyRepository repository, ILogger<GetEnvironmentalChemicalSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalChemicalSafetyQueryResult>> Handle(GetEnvironmentalChemicalSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEnvironmentalChemicalSafetyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EnvironmentalChemicalSafetyQueryResult>.Success(new EnvironmentalChemicalSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnvironmentalChemicalSafety for Tenant {TenantId}", request.TenantId);

            return Result<EnvironmentalChemicalSafetyQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.EnvironmentalChemicalSafety.RetrievalFailed",
                "Unable to retrieve EnvironmentalChemicalSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}