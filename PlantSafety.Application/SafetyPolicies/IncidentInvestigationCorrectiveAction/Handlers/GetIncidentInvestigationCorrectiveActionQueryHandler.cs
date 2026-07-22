// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetIncidentInvestigationCorrectiveActionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Queries;
using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Repositories;

namespace PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Handlers;

/// <summary>Handles <see cref="GetIncidentInvestigationCorrectiveActionQuery"/>.</summary>
public sealed class GetIncidentInvestigationCorrectiveActionQueryHandler : IRequestHandler<GetIncidentInvestigationCorrectiveActionQuery, Result<IncidentInvestigationCorrectiveActionQueryResult>>
{
    private readonly IIncidentInvestigationCorrectiveActionRepository _repository;
    private readonly ILogger<GetIncidentInvestigationCorrectiveActionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentInvestigationCorrectiveActionQueryHandler(IIncidentInvestigationCorrectiveActionRepository repository, ILogger<GetIncidentInvestigationCorrectiveActionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentInvestigationCorrectiveActionQueryResult>> Handle(GetIncidentInvestigationCorrectiveActionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentInvestigationCorrectiveActionAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IncidentInvestigationCorrectiveActionQueryResult>.Success(new IncidentInvestigationCorrectiveActionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentInvestigationCorrectiveAction for Tenant {TenantId}", request.TenantId);

            return Result<IncidentInvestigationCorrectiveActionQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.IncidentInvestigationCorrectiveAction.RetrievalFailed",
                "Unable to retrieve IncidentInvestigationCorrectiveAction data at this time.",
                Guid.NewGuid()));
        }
    }
}