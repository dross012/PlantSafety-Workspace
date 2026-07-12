// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetAllInvestigationsQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.AllInvestigations.Queries;
using PlantSafety.Application.Intelligence.AllInvestigations.Repositories;

namespace PlantSafety.Application.Intelligence.AllInvestigations.Handlers;

/// <summary>Handles <see cref="GetAllInvestigationsQuery"/>.</summary>
public sealed class GetAllInvestigationsQueryHandler : IRequestHandler<GetAllInvestigationsQuery, Result<AllInvestigationsQueryResult>>
{
    private readonly IAllInvestigationsRepository _repository;
    private readonly ILogger<GetAllInvestigationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAllInvestigationsQueryHandler(IAllInvestigationsRepository repository, ILogger<GetAllInvestigationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AllInvestigationsQueryResult>> Handle(GetAllInvestigationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInvestigationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AllInvestigationsQueryResult>.Success(new AllInvestigationsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AllInvestigations data for Tenant {TenantId}", request.TenantId);

            return Result<AllInvestigationsQueryResult>.Failure(new ApplicationError(
                "Intelligence.AllInvestigations.RetrievalFailed",
                "Unable to retrieve AllInvestigations data at this time.",
                Guid.NewGuid()));
        }
    }
}