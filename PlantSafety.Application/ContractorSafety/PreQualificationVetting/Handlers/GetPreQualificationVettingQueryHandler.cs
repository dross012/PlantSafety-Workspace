// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPreQualificationVettingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorSafety.PreQualificationVetting.Queries;
using PlantSafety.Application.ContractorSafety.PreQualificationVetting.Repositories;

namespace PlantSafety.Application.ContractorSafety.PreQualificationVetting.Handlers;

/// <summary>Handles <see cref="GetPreQualificationVettingQuery"/>.</summary>
public sealed class GetPreQualificationVettingQueryHandler : IRequestHandler<GetPreQualificationVettingQuery, Result<PreQualificationVettingQueryResult>>
{
    private readonly IPreQualificationVettingRepository _repository;
    private readonly ILogger<GetPreQualificationVettingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPreQualificationVettingQueryHandler(IPreQualificationVettingRepository repository, ILogger<GetPreQualificationVettingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreQualificationVettingQueryResult>> Handle(GetPreQualificationVettingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPreQualificationVettingItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PreQualificationVettingQueryResult>.Success(new PreQualificationVettingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PreQualificationVetting records for Tenant {TenantId}", request.TenantId);

            return Result<PreQualificationVettingQueryResult>.Failure(new ApplicationError(
                "ContractorSafety.PreQualificationVetting.RetrievalFailed",
                "Unable to retrieve PreQualificationVetting records at this time.",
                Guid.NewGuid()));
        }
    }
}