// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetInternalQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Notifications.Internal.Queries;
using PlantSafety.Application.Integrations.Notifications.Internal.Repositories;

namespace PlantSafety.Application.Integrations.Notifications.Internal.Handlers;

/// <summary>Handles <see cref="GetInternalQuery"/>.</summary>
public sealed class GetInternalQueryHandler : IRequestHandler<GetInternalQuery, Result<InternalQueryResult>>
{
    private readonly IInternalRepository _repository;
    private readonly ILogger<GetInternalQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetInternalQueryHandler(IInternalRepository repository, ILogger<GetInternalQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<InternalQueryResult>> Handle(GetInternalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInternalNotificationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<InternalQueryResult>.Success(new InternalQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Internal data for Tenant {TenantId}", request.TenantId);

            return Result<InternalQueryResult>.Failure(new ApplicationError(
                "Integrations.Notifications.Internal.RetrievalFailed",
                "Unable to retrieve Internal data at this time.",
                Guid.NewGuid()));
        }
    }
}