// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCAPAQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContinuousImprovement.CAPA.Queries;
using PlantSafety.Application.ContinuousImprovement.CAPA.Repositories;

namespace PlantSafety.Application.ContinuousImprovement.CAPA.Handlers;

/// <summary>Handles <see cref="GetCAPAQuery"/>.</summary>
public sealed class GetCAPAQueryHandler : IRequestHandler<GetCAPAQuery, Result<CAPAQueryResult>>
{
    private readonly ICAPARepository _repository;
    private readonly ILogger<GetCAPAQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCAPAQueryHandler(ICAPARepository repository, ILogger<GetCAPAQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CAPAQueryResult>> Handle(GetCAPAQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetActionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CAPAQueryResult>.Success(new CAPAQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CAPA for Tenant {TenantId}", request.TenantId);

            return Result<CAPAQueryResult>.Failure(new ApplicationError(
                "ContinuousImprovement.CAPA.RetrievalFailed",
                "Unable to retrieve CAPA data at this time.",
                Guid.NewGuid()));
        }
    }
}
