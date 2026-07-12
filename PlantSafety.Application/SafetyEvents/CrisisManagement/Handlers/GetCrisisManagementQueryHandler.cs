// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCrisisManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Queries;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Handlers;

/// <summary>Handles <see cref="GetCrisisManagementQuery"/>.</summary>
public sealed class GetCrisisManagementQueryHandler : IRequestHandler<GetCrisisManagementQuery, Result<CrisisManagementQueryResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<GetCrisisManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCrisisManagementQueryHandler(ICrisisManagementRepository repository, ILogger<GetCrisisManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementQueryResult>> Handle(GetCrisisManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCrisisEventsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CrisisManagementQueryResult>.Success(new CrisisManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CrisisManagement for Tenant {TenantId}", request.TenantId);

            return Result<CrisisManagementQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.RetrievalFailed",
                "Unable to retrieve crisis management data at this time.",
                Guid.NewGuid()));
        }
    }
}
