// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetContractorQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Contractor.Queries;
using PlantSafety.Application.Operational.Contractor.Repositories;

namespace PlantSafety.Application.Operational.Contractor.Handlers;

/// <summary>Handles <see cref="GetContractorQuery"/>.</summary>
public sealed class GetContractorQueryHandler : IRequestHandler<GetContractorQuery, Result<ContractorQueryResult>>
{
    private readonly IContractorRepository _repository;
    private readonly ILogger<GetContractorQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetContractorQueryHandler(IContractorRepository repository, ILogger<GetContractorQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContractorQueryResult>> Handle(GetContractorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetContractorAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ContractorQueryResult>.Success(new ContractorQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Contractor for Tenant {TenantId}", request.TenantId);

            return Result<ContractorQueryResult>.Failure(new ApplicationError(
                "Operational.Contractor.RetrievalFailed",
                "Unable to retrieve Contractor data at this time.",
                Guid.NewGuid()));
        }
    }
}
