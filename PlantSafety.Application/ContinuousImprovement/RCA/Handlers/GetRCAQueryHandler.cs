// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRCAQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContinuousImprovement.RCA.Queries;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Handlers;

/// <summary>Handles <see cref="GetRCAQuery"/>.</summary>
public sealed class GetRCAQueryHandler : IRequestHandler<GetRCAQuery, Result<RCAQueryResult>>
{
    private readonly IRCARepository _repository;
    private readonly ILogger<GetRCAQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRCAQueryHandler(IRCARepository repository, ILogger<GetRCAQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RCAQueryResult>> Handle(GetRCAQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRcaRecordsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RCAQueryResult>.Success(new RCAQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RCA for Tenant {TenantId}", request.TenantId);

            return Result<RCAQueryResult>.Failure(new ApplicationError(
                "ContinuousImprovement.RCA.RetrievalFailed",
                "Unable to retrieve RCA data at this time.",
                Guid.NewGuid()));
        }
    }
}
