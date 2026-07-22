// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPPECategoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PPE.PPECategory.Queries;
using PlantSafety.Application.PPE.PPECategory.Repositories;

namespace PlantSafety.Application.PPE.PPECategory.Handlers;

/// <summary>Handles <see cref="GetPPECategoryQuery"/>.</summary>
public sealed class GetPPECategoryQueryHandler : IRequestHandler<GetPPECategoryQuery, Result<PPECategoryQueryResult>>
{
    private readonly IPPECategoryRepository _repository;
    private readonly ILogger<GetPPECategoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPECategoryQueryHandler(IPPECategoryRepository repository, ILogger<GetPPECategoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPECategoryQueryResult>> Handle(GetPPECategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPECategoryAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPECategoryQueryResult>.Success(new PPECategoryQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPECategory for Tenant {TenantId}", request.TenantId);

            return Result<PPECategoryQueryResult>.Failure(new ApplicationError(
                "PPE.PPECategory.RetrievalFailed",
                "Unable to retrieve PPECategory data at this time.",
                Guid.NewGuid()));
        }
    }
}