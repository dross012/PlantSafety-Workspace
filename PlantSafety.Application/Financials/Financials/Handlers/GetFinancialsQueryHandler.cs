// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetFinancialsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Financials.Financials.Queries;
using PlantSafety.Application.Financials.Financials.Repositories;

namespace PlantSafety.Application.Financials.Financials.Handlers;

/// <summary>Handles <see cref="GetFinancialsQuery"/>.</summary>
public sealed class GetFinancialsQueryHandler : IRequestHandler<GetFinancialsQuery, Result<FinancialsQueryResult>>
{
    private readonly IFinancialsRepository _repository;
    private readonly ILogger<GetFinancialsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFinancialsQueryHandler(IFinancialsRepository repository, ILogger<GetFinancialsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FinancialsQueryResult>> Handle(GetFinancialsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRecordsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<FinancialsQueryResult>.Success(new FinancialsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Financials for Tenant {TenantId}", request.TenantId);

            return Result<FinancialsQueryResult>.Failure(new ApplicationError(
                "Financials.Financials.RetrievalFailed",
                "Unable to retrieve financial data at this time.",
                Guid.NewGuid()));
        }
    }
}
