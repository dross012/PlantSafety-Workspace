// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPreStartupSafetyReviewQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Queries;
using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Repositories;

namespace PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Handlers;

/// <summary>Handles <see cref="GetPreStartupSafetyReviewQuery"/>.</summary>
public sealed class GetPreStartupSafetyReviewQueryHandler : IRequestHandler<GetPreStartupSafetyReviewQuery, Result<PreStartupSafetyReviewQueryResult>>
{
    private readonly IPreStartupSafetyReviewRepository _repository;
    private readonly ILogger<GetPreStartupSafetyReviewQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPreStartupSafetyReviewQueryHandler(IPreStartupSafetyReviewRepository repository, ILogger<GetPreStartupSafetyReviewQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreStartupSafetyReviewQueryResult>> Handle(GetPreStartupSafetyReviewQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPreStartupSafetyReviewAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PreStartupSafetyReviewQueryResult>.Success(new PreStartupSafetyReviewQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PreStartupSafetyReview for Tenant {TenantId}", request.TenantId);

            return Result<PreStartupSafetyReviewQueryResult>.Failure(new ApplicationError(
                "RiskManagement.PreStartupSafetyReview.RetrievalFailed",
                "Unable to retrieve PreStartupSafetyReview data at this time.",
                Guid.NewGuid()));
        }
    }
}