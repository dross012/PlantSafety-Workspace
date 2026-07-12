// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetUpdatesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Updates.Queries;
using PlantSafety.Application.CrossDomain.Updates.Repositories;

namespace PlantSafety.Application.CrossDomain.Updates.Handlers;

/// <summary>Handles <see cref="GetUpdatesQuery"/>.</summary>
public sealed class GetUpdatesQueryHandler : IRequestHandler<GetUpdatesQuery, Result<UpdatesQueryResult>>
{
    private readonly IUpdatesRepository _repository;
    private readonly ILogger<GetUpdatesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetUpdatesQueryHandler(IUpdatesRepository repository, ILogger<GetUpdatesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<UpdatesQueryResult>> Handle(GetUpdatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetUpdatesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<UpdatesQueryResult>.Success(new UpdatesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Updates for Tenant {TenantId}", request.TenantId);

            return Result<UpdatesQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Updates.RetrievalFailed",
                "Unable to retrieve updates data at this time.",
                Guid.NewGuid()));
        }
    }
}
