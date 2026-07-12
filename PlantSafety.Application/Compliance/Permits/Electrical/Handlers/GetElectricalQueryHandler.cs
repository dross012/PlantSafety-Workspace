// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetElectricalQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Electrical.Queries;
using PlantSafety.Application.Compliance.Permits.Electrical.Repositories;

namespace PlantSafety.Application.Compliance.Permits.Electrical.Handlers;

/// <summary>Handles <see cref="GetElectricalQuery"/>.</summary>
public sealed class GetElectricalQueryHandler : IRequestHandler<GetElectricalQuery, Result<ElectricalQueryResult>>
{
    private readonly IElectricalRepository _repository;
    private readonly ILogger<GetElectricalQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetElectricalQueryHandler(IElectricalRepository repository, ILogger<GetElectricalQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ElectricalQueryResult>> Handle(GetElectricalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetElectricalAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ElectricalQueryResult>.Success(new ElectricalQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Electrical for Tenant {TenantId}", request.TenantId);

            return Result<ElectricalQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.Electrical.RetrievalFailed",
                "Unable to retrieve electrical work permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
