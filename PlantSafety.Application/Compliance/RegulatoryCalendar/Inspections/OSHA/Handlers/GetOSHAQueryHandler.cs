// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetOSHAQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Handlers;

/// <summary>Handles <see cref="GetOSHAQuery"/>.</summary>
public sealed class GetOSHAQueryHandler : IRequestHandler<GetOSHAQuery, Result<OSHAQueryResult>>
{
    private readonly IOSHARepository _repository;
    private readonly ILogger<GetOSHAQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOSHAQueryHandler(IOSHARepository repository, ILogger<GetOSHAQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OSHAQueryResult>> Handle(GetOSHAQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInspectionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<OSHAQueryResult>.Success(new OSHAQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve OSHA for Tenant {TenantId}", request.TenantId);

            return Result<OSHAQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.Inspections.OSHA.RetrievalFailed",
                "Unable to retrieve OSHA data at this time.",
                Guid.NewGuid()));
        }
    }
}
