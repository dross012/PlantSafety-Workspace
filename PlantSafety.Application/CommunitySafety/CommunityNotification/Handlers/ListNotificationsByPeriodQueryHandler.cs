// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListNotificationsByPeriodQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Queries;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Repositories;

namespace PlantSafety.Application.CommunitySafety.CommunityNotification.Handlers;

/// <summary>Handles <see cref="ListNotificationsByPeriodQuery"/>.</summary>
public sealed class ListNotificationsByPeriodQueryHandler : IRequestHandler<ListNotificationsByPeriodQuery, Result<CommunityNotificationsQueryResult>>
{
    private readonly ICommunityNotificationRepository _repository;
    private readonly ILogger<ListNotificationsByPeriodQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListNotificationsByPeriodQueryHandler(ICommunityNotificationRepository repository, ILogger<ListNotificationsByPeriodQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityNotificationsQueryResult>> Handle(ListNotificationsByPeriodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCommunityNotificationItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CommunityNotificationsQueryResult>.Success(new CommunityNotificationsQueryResult(
                request.TenantId, request.SiteId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list CommunityNotification records for Tenant {TenantId}", request.TenantId);

            return Result<CommunityNotificationsQueryResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityNotification.RetrievalFailed",
                "Unable to list CommunityNotification records at this time.",
                Guid.NewGuid()));
        }
    }
}
