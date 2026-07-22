// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSystemsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.Systems.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Systems.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.Systems.Handlers;

/// <summary>Handles <see cref="GetSystemsQuery"/>.</summary>
public sealed class GetSystemsQueryHandler : IRequestHandler<GetSystemsQuery, Result<SystemsQueryResult>>
{
    private readonly ISystemsRepository _repository;
    private readonly ILogger<GetSystemsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSystemsQueryHandler(ISystemsRepository repository, ILogger<GetSystemsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SystemsQueryResult>> Handle(GetSystemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SystemsQueryResult>.Success(new SystemsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Systems for Tenant {TenantId}", request.TenantId);

            return Result<SystemsQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.Systems.RetrievalFailed",
                "Unable to retrieve Systems Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}