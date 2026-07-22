// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSafetyTrainingReportingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Queries;
using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Repositories;

namespace PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Handlers;

/// <summary>Handles <see cref="GetSafetyTrainingReportingQuery"/>.</summary>
public sealed class GetSafetyTrainingReportingQueryHandler : IRequestHandler<GetSafetyTrainingReportingQuery, Result<SafetyTrainingReportingQueryResult>>
{
    private readonly ISafetyTrainingReportingRepository _repository;
    private readonly ILogger<GetSafetyTrainingReportingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyTrainingReportingQueryHandler(ISafetyTrainingReportingRepository repository, ILogger<GetSafetyTrainingReportingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyTrainingReportingQueryResult>> Handle(GetSafetyTrainingReportingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSafetyTrainingReportingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetyTrainingReportingQueryResult>.Success(new SafetyTrainingReportingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyTrainingReporting for Tenant {TenantId}", request.TenantId);

            return Result<SafetyTrainingReportingQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.SafetyTrainingReporting.RetrievalFailed",
                "Unable to retrieve SafetyTrainingReporting data at this time.",
                Guid.NewGuid()));
        }
    }
}