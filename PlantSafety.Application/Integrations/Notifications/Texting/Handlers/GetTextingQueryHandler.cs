// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetTextingQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Notifications.Texting.Queries;
using PlantSafety.Application.Integrations.Notifications.Texting.Repositories;

namespace PlantSafety.Application.Integrations.Notifications.Texting.Handlers;

/// <summary>Handles <see cref="GetTextingQuery"/>.</summary>
public sealed class GetTextingQueryHandler : IRequestHandler<GetTextingQuery, Result<TextingQueryResult>>
{
    private readonly ITextingRepository _repository;
    private readonly ILogger<GetTextingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTextingQueryHandler(ITextingRepository repository, ILogger<GetTextingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TextingQueryResult>> Handle(GetTextingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTextingDeliveriesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TextingQueryResult>.Success(new TextingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Texting data for Tenant {TenantId}", request.TenantId);

            return Result<TextingQueryResult>.Failure(new ApplicationError(
                "Integrations.Notifications.Texting.RetrievalFailed",
                "Unable to retrieve Texting data at this time.",
                Guid.NewGuid()));
        }
    }
}