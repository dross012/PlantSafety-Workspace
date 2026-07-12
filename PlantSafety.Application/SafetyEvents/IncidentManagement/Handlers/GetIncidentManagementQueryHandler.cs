// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetIncidentManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.IncidentManagement.Queries;
using PlantSafety.Application.SafetyEvents.IncidentManagement.Repositories;

namespace PlantSafety.Application.SafetyEvents.IncidentManagement.Handlers;

/// <summary>Handles <see cref="GetIncidentManagementQuery"/>.</summary>
public sealed class GetIncidentManagementQueryHandler : IRequestHandler<GetIncidentManagementQuery, Result<IncidentManagementQueryResult>>
{
    private readonly IIncidentManagementRepository _repository;
    private readonly ILogger<GetIncidentManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentManagementQueryHandler(IIncidentManagementRepository repository, ILogger<GetIncidentManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentManagementQueryResult>> Handle(GetIncidentManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IncidentManagementQueryResult>.Success(new IncidentManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentManagement for Tenant {TenantId}", request.TenantId);

            return Result<IncidentManagementQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.IncidentManagement.RetrievalFailed",
                "Unable to retrieve incident management data at this time.",
                Guid.NewGuid()));
        }
    }
}
