// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCommunicationLogByIncidentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Queries;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Repositories;

namespace PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Handlers;

/// <summary>Handles <see cref="GetCommunicationLogByIncidentQuery"/>.</summary>
public sealed class GetCommunicationLogByIncidentQueryHandler : IRequestHandler<GetCommunicationLogByIncidentQuery, Result<GetCommunicationLogByIncidentQueryResult>>
{
    private readonly ICommunicationLogEntryRepository _repository;
    private readonly ILogger<GetCommunicationLogByIncidentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCommunicationLogByIncidentQueryHandler(ICommunicationLogEntryRepository repository, ILogger<GetCommunicationLogByIncidentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetCommunicationLogByIncidentQueryResult>> Handle(GetCommunicationLogByIncidentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCommunicationLogByIncidentAsync(request.TenantId, request.IncidentId, cancellationToken);

            return Result<GetCommunicationLogByIncidentQueryResult>.Success(new GetCommunicationLogByIncidentQueryResult(
                request.TenantId,
                request.IncidentId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve communication log for Incident {IncidentId}", request.IncidentId);

            return Result<GetCommunicationLogByIncidentQueryResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.CommunicationLogEntry.RetrievalFailed",
                "Unable to retrieve the communication log at this time.",
                Guid.NewGuid()));
        }
    }
}
