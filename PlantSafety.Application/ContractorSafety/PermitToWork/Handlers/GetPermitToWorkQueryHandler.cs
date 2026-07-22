// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPermitToWorkQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorSafety.PermitToWork.Queries;
using PlantSafety.Application.ContractorSafety.PermitToWork.Repositories;

namespace PlantSafety.Application.ContractorSafety.PermitToWork.Handlers;

/// <summary>Handles <see cref="GetPermitToWorkQuery"/>.</summary>
public sealed class GetPermitToWorkQueryHandler : IRequestHandler<GetPermitToWorkQuery, Result<PermitToWorkQueryResult>>
{
    private readonly IPermitToWorkRepository _repository;
    private readonly ILogger<GetPermitToWorkQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitToWorkQueryHandler(IPermitToWorkRepository repository, ILogger<GetPermitToWorkQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitToWorkQueryResult>> Handle(GetPermitToWorkQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitToWorkItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitToWorkQueryResult>.Success(new PermitToWorkQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PermitToWork records for Tenant {TenantId}", request.TenantId);

            return Result<PermitToWorkQueryResult>.Failure(new ApplicationError(
                "ContractorSafety.PermitToWork.RetrievalFailed",
                "Unable to retrieve PermitToWork records at this time.",
                Guid.NewGuid()));
        }
    }
}