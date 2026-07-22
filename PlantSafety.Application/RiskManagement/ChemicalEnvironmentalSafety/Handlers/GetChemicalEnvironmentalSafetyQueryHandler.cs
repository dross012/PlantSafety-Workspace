// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetChemicalEnvironmentalSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Queries;
using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Repositories;

namespace PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Handlers;

/// <summary>Handles <see cref="GetChemicalEnvironmentalSafetyQuery"/>.</summary>
public sealed class GetChemicalEnvironmentalSafetyQueryHandler : IRequestHandler<GetChemicalEnvironmentalSafetyQuery, Result<ChemicalEnvironmentalSafetyQueryResult>>
{
    private readonly IChemicalEnvironmentalSafetyRepository _repository;
    private readonly ILogger<GetChemicalEnvironmentalSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetChemicalEnvironmentalSafetyQueryHandler(IChemicalEnvironmentalSafetyRepository repository, ILogger<GetChemicalEnvironmentalSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ChemicalEnvironmentalSafetyQueryResult>> Handle(GetChemicalEnvironmentalSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetChemicalEnvironmentalSafetyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ChemicalEnvironmentalSafetyQueryResult>.Success(new ChemicalEnvironmentalSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ChemicalEnvironmentalSafety for Tenant {TenantId}", request.TenantId);

            return Result<ChemicalEnvironmentalSafetyQueryResult>.Failure(new ApplicationError(
                "RiskManagement.ChemicalEnvironmentalSafety.RetrievalFailed",
                "Unable to retrieve ChemicalEnvironmentalSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}