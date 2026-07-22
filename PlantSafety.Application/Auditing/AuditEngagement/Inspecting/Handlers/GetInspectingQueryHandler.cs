// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetInspectingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Handlers;

/// <summary>Handles <see cref="GetInspectingQuery"/>.</summary>
public sealed class GetInspectingQueryHandler : IRequestHandler<GetInspectingQuery, Result<InspectingQueryResult>>
{
    private readonly IInspectingRepository _repository;
    private readonly ILogger<GetInspectingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetInspectingQueryHandler(IInspectingRepository repository, ILogger<GetInspectingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<InspectingQueryResult>> Handle(GetInspectingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<InspectingQueryResult>.Success(new InspectingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Inspecting for Tenant {TenantId}", request.TenantId);

            return Result<InspectingQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.Inspecting.RetrievalFailed",
                "Unable to retrieve Inspecting Audits data at this time.",
                Guid.NewGuid()));
        }
    }
}