// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetISOQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Handlers;

/// <summary>Handles <see cref="GetISOQuery"/>.</summary>
public sealed class GetISOQueryHandler : IRequestHandler<GetISOQuery, Result<ISOQueryResult>>
{
    private readonly IISORepository _repository;
    private readonly ILogger<GetISOQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetISOQueryHandler(IISORepository repository, ILogger<GetISOQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ISOQueryResult>> Handle(GetISOQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRenewalsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ISOQueryResult>.Success(new ISOQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ISO for Tenant {TenantId}", request.TenantId);

            return Result<ISOQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.Renewals.ISO.RetrievalFailed",
                "Unable to retrieve ISO data at this time.",
                Guid.NewGuid()));
        }
    }
}
