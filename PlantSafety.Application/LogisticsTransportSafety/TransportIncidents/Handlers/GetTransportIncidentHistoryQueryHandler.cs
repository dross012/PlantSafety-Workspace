// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetTransportIncidentHistoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Repositories;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Handlers;

/// <summary>Handles <see cref="GetTransportIncidentHistoryQuery"/>.</summary>
public sealed class GetTransportIncidentHistoryQueryHandler : IRequestHandler<GetTransportIncidentHistoryQuery, Result<TransportIncidentQueryResult>>
{
    private readonly ITransportIncidentRepository _repository;
    private readonly ILogger<GetTransportIncidentHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTransportIncidentHistoryQueryHandler(ITransportIncidentRepository repository, ILogger<GetTransportIncidentHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportIncidentQueryResult>> Handle(GetTransportIncidentHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTransportIncidentHistoryAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TransportIncidentQueryResult>.Success(new TransportIncidentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TransportIncident history for Tenant {TenantId}", request.TenantId);

            return Result<TransportIncidentQueryResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportIncidents.RetrievalFailed",
                "Unable to retrieve TransportIncident history at this time.",
                Guid.NewGuid()));
        }
    }
}
