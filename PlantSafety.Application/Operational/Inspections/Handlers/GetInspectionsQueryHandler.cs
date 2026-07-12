// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetInspectionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Inspections.Queries;
using PlantSafety.Application.Operational.Inspections.Repositories;

namespace PlantSafety.Application.Operational.Inspections.Handlers;

/// <summary>Handles <see cref="GetInspectionsQuery"/>.</summary>
public sealed class GetInspectionsQueryHandler : IRequestHandler<GetInspectionsQuery, Result<InspectionsQueryResult>>
{
    private readonly IInspectionsRepository _repository;
    private readonly ILogger<GetInspectionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetInspectionsQueryHandler(IInspectionsRepository repository, ILogger<GetInspectionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<InspectionsQueryResult>> Handle(GetInspectionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInspectionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<InspectionsQueryResult>.Success(new InspectionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Inspections for Tenant {TenantId}", request.TenantId);

            return Result<InspectionsQueryResult>.Failure(new ApplicationError(
                "Operational.Inspections.RetrievalFailed",
                "Unable to retrieve Inspections data at this time.",
                Guid.NewGuid()));
        }
    }
}
