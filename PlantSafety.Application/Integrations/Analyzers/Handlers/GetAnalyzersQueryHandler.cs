// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetAnalyzersQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Analyzers.Queries;
using PlantSafety.Application.Integrations.Analyzers.Repositories;

namespace PlantSafety.Application.Integrations.Analyzers.Handlers;

/// <summary>Handles <see cref="GetAnalyzersQuery"/>.</summary>
public sealed class GetAnalyzersQueryHandler : IRequestHandler<GetAnalyzersQuery, Result<AnalyzersQueryResult>>
{
    private readonly IAnalyzersRepository _repository;
    private readonly ILogger<GetAnalyzersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAnalyzersQueryHandler(IAnalyzersRepository repository, ILogger<GetAnalyzersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AnalyzersQueryResult>> Handle(GetAnalyzersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRunsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AnalyzersQueryResult>.Success(new AnalyzersQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Analyzers for Tenant {TenantId}", request.TenantId);

            return Result<AnalyzersQueryResult>.Failure(new ApplicationError(
                "Integrations.Analyzers.RetrievalFailed",
                "Unable to retrieve integration analyzer run results at this time.",
                Guid.NewGuid()));
        }
    }
}
