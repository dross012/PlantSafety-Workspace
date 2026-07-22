// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetContractorSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Handlers;

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
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

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
                "Checklists.ChecklistTemplate.ContractorSafety.RetrievalFailed",
                "Unable to retrieve Contractor Safety Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}