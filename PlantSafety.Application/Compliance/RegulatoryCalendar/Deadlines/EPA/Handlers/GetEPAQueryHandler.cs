// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEPAQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Handlers;

/// <summary>Handles <see cref="GetEPAQuery"/>.</summary>
public sealed class GetEPAQueryHandler : IRequestHandler<GetEPAQuery, Result<EPAQueryResult>>
{
    private readonly IEPARepository _repository;
    private readonly ILogger<GetEPAQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEPAQueryHandler(IEPARepository repository, ILogger<GetEPAQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EPAQueryResult>> Handle(GetEPAQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDeadlinesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EPAQueryResult>.Success(new EPAQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EPA for Tenant {TenantId}", request.TenantId);

            return Result<EPAQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.Deadlines.EPA.RetrievalFailed",
                "Unable to retrieve EPA data at this time.",
                Guid.NewGuid()));
        }
    }
}
