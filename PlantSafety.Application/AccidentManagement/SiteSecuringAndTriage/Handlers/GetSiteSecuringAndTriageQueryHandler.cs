// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSiteSecuringAndTriageQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Queries;
using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Repositories;

namespace PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Handlers;

/// <summary>Handles <see cref="GetSiteSecuringAndTriageQuery"/>.</summary>
public sealed class GetSiteSecuringAndTriageQueryHandler : IRequestHandler<GetSiteSecuringAndTriageQuery, Result<SiteSecuringAndTriageQueryResult>>
{
    private readonly ISiteSecuringAndTriageRepository _repository;
    private readonly ILogger<GetSiteSecuringAndTriageQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSiteSecuringAndTriageQueryHandler(ISiteSecuringAndTriageRepository repository, ILogger<GetSiteSecuringAndTriageQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SiteSecuringAndTriageQueryResult>> Handle(GetSiteSecuringAndTriageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSiteSecuringAndTriageAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SiteSecuringAndTriageQueryResult>.Success(new SiteSecuringAndTriageQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SiteSecuringAndTriage for Tenant {TenantId}", request.TenantId);

            return Result<SiteSecuringAndTriageQueryResult>.Failure(new ApplicationError(
                "AccidentManagement.SiteSecuringAndTriage.RetrievalFailed",
                "Unable to retrieve SiteSecuringAndTriage data at this time.",
                Guid.NewGuid()));
        }
    }
}