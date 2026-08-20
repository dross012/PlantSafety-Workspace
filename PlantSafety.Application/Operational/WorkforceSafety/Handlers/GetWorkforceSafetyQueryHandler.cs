// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Handler for GetWorkforceSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.WorkforceSafety.Queries;
using PlantSafety.Application.Operational.WorkforceSafety.Repositories;

namespace PlantSafety.Application.Operational.WorkforceSafety.Handlers;

/// <summary>Handles <see cref="GetWorkforceSafetyQuery"/>.</summary>
public sealed class GetWorkforceSafetyQueryHandler : IRequestHandler<GetWorkforceSafetyQuery, Result<WorkforceSafetyQueryResult>>
{
    private readonly IWorkforceSafetyRepository _repository;
    private readonly ILogger<GetWorkforceSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkforceSafetyQueryHandler(IWorkforceSafetyRepository repository, ILogger<GetWorkforceSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkforceSafetyQueryResult>> Handle(GetWorkforceSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWorkforceSafetyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WorkforceSafetyQueryResult>.Success(new WorkforceSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkforceSafety for Tenant {TenantId}", request.TenantId);

            return Result<WorkforceSafetyQueryResult>.Failure(new ApplicationError(
                "Operational.WorkforceSafety.RetrievalFailed",
                "Unable to retrieve WorkforceSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}