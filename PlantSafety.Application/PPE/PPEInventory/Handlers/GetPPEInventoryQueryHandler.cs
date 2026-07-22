// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPPEInventoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PPE.PPEInventory.Queries;
using PlantSafety.Application.PPE.PPEInventory.Repositories;

namespace PlantSafety.Application.PPE.PPEInventory.Handlers;

/// <summary>Handles <see cref="GetPPEInventoryQuery"/>.</summary>
public sealed class GetPPEInventoryQueryHandler : IRequestHandler<GetPPEInventoryQuery, Result<PPEInventoryQueryResult>>
{
    private readonly IPPEInventoryRepository _repository;
    private readonly ILogger<GetPPEInventoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPEInventoryQueryHandler(IPPEInventoryRepository repository, ILogger<GetPPEInventoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEInventoryQueryResult>> Handle(GetPPEInventoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPEInventoryAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPEInventoryQueryResult>.Success(new PPEInventoryQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPEInventory for Tenant {TenantId}", request.TenantId);

            return Result<PPEInventoryQueryResult>.Failure(new ApplicationError(
                "PPE.PPEInventory.RetrievalFailed",
                "Unable to retrieve PPEInventory data at this time.",
                Guid.NewGuid()));
        }
    }
}