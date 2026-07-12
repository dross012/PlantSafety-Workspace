// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEnvironmentalQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Environmental.Queries;
using PlantSafety.Application.Compliance.Policy.Environmental.Repositories;

namespace PlantSafety.Application.Compliance.Policy.Environmental.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalQuery"/>.</summary>
public sealed class GetEnvironmentalQueryHandler : IRequestHandler<GetEnvironmentalQuery, Result<EnvironmentalQueryResult>>
{
    private readonly IEnvironmentalRepository _repository;
    private readonly ILogger<GetEnvironmentalQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalQueryHandler(IEnvironmentalRepository repository, ILogger<GetEnvironmentalQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalQueryResult>> Handle(GetEnvironmentalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEnvironmentalPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EnvironmentalQueryResult>.Success(new EnvironmentalQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Environmental for Tenant {TenantId}", request.TenantId);

            return Result<EnvironmentalQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.Environmental.RetrievalFailed",
                "Unable to retrieve Environmental data at this time.",
                Guid.NewGuid()));
        }
    }
}
