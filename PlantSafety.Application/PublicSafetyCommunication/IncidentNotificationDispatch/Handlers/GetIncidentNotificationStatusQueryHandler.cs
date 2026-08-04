// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetIncidentNotificationStatusQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Queries;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Repositories;

namespace PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Handlers;

/// <summary>Handles <see cref="GetIncidentNotificationStatusQuery"/>.</summary>
public sealed class GetIncidentNotificationStatusQueryHandler : IRequestHandler<GetIncidentNotificationStatusQuery, Result<GetIncidentNotificationStatusQueryResult>>
{
    private readonly IIncidentNotificationRepository _repository;
    private readonly ILogger<GetIncidentNotificationStatusQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentNotificationStatusQueryHandler(IIncidentNotificationRepository repository, ILogger<GetIncidentNotificationStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetIncidentNotificationStatusQueryResult>> Handle(GetIncidentNotificationStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentNotificationStatusAsync(request.TenantId, request.SiteId, request.IncidentId, request.From, request.To, cancellationToken);

            return Result<GetIncidentNotificationStatusQueryResult>.Success(new GetIncidentNotificationStatusQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentNotification status for Tenant {TenantId}", request.TenantId);

            return Result<GetIncidentNotificationStatusQueryResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.IncidentNotification.StatusRetrievalFailed",
                "Unable to retrieve incident notification status at this time.",
                Guid.NewGuid()));
        }
    }
}
