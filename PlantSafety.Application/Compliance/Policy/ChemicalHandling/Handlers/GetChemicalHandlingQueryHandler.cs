// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetChemicalHandlingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.ChemicalHandling.Queries;
using PlantSafety.Application.Compliance.Policy.ChemicalHandling.Repositories;

namespace PlantSafety.Application.Compliance.Policy.ChemicalHandling.Handlers;

/// <summary>Handles <see cref="GetChemicalHandlingQuery"/>.</summary>
public sealed class GetChemicalHandlingQueryHandler : IRequestHandler<GetChemicalHandlingQuery, Result<ChemicalHandlingQueryResult>>
{
    private readonly IChemicalHandlingRepository _repository;
    private readonly ILogger<GetChemicalHandlingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetChemicalHandlingQueryHandler(IChemicalHandlingRepository repository, ILogger<GetChemicalHandlingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ChemicalHandlingQueryResult>> Handle(GetChemicalHandlingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetChemicalHandlingPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ChemicalHandlingQueryResult>.Success(new ChemicalHandlingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ChemicalHandling for Tenant {TenantId}", request.TenantId);

            return Result<ChemicalHandlingQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.ChemicalHandling.RetrievalFailed",
                "Unable to retrieve ChemicalHandling data at this time.",
                Guid.NewGuid()));
        }
    }
}
