// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetObligationsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Obligations.Queries;
using PlantSafety.Application.Compliance.Obligations.Repositories;

namespace PlantSafety.Application.Compliance.Obligations.Handlers;

/// <summary>Handles <see cref="GetObligationsQuery"/>.</summary>
public sealed class GetObligationsQueryHandler : IRequestHandler<GetObligationsQuery, Result<ObligationsQueryResult>>
{
    private readonly IObligationsRepository _repository;
    private readonly ILogger<GetObligationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetObligationsQueryHandler(IObligationsRepository repository, ILogger<GetObligationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ObligationsQueryResult>> Handle(GetObligationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetObligationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ObligationsQueryResult>.Success(new ObligationsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Obligations for Tenant {TenantId}", request.TenantId);

            return Result<ObligationsQueryResult>.Failure(new ApplicationError(
                "Compliance.Obligations.RetrievalFailed",
                "Unable to retrieve compliance obligations data at this time.",
                Guid.NewGuid()));
        }
    }
}
