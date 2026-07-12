// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetColdWorkQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.ColdWork.Queries;
using PlantSafety.Application.Compliance.Permits.ColdWork.Repositories;

namespace PlantSafety.Application.Compliance.Permits.ColdWork.Handlers;

/// <summary>Handles <see cref="GetColdWorkQuery"/>.</summary>
public sealed class GetColdWorkQueryHandler : IRequestHandler<GetColdWorkQuery, Result<ColdWorkQueryResult>>
{
    private readonly IColdWorkRepository _repository;
    private readonly ILogger<GetColdWorkQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetColdWorkQueryHandler(IColdWorkRepository repository, ILogger<GetColdWorkQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ColdWorkQueryResult>> Handle(GetColdWorkQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetColdWorkAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ColdWorkQueryResult>.Success(new ColdWorkQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ColdWork for Tenant {TenantId}", request.TenantId);

            return Result<ColdWorkQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.ColdWork.RetrievalFailed",
                "Unable to retrieve cold work permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
