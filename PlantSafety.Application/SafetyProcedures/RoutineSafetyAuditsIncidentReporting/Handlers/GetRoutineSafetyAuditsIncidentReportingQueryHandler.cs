// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetRoutineSafetyAuditsIncidentReportingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Queries;
using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Repositories;

namespace PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Handlers;

/// <summary>Handles <see cref="GetRoutineSafetyAuditsIncidentReportingQuery"/>.</summary>
public sealed class GetRoutineSafetyAuditsIncidentReportingQueryHandler : IRequestHandler<GetRoutineSafetyAuditsIncidentReportingQuery, Result<RoutineSafetyAuditsIncidentReportingQueryResult>>
{
    private readonly IRoutineSafetyAuditsIncidentReportingRepository _repository;
    private readonly ILogger<GetRoutineSafetyAuditsIncidentReportingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRoutineSafetyAuditsIncidentReportingQueryHandler(IRoutineSafetyAuditsIncidentReportingRepository repository, ILogger<GetRoutineSafetyAuditsIncidentReportingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RoutineSafetyAuditsIncidentReportingQueryResult>> Handle(GetRoutineSafetyAuditsIncidentReportingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRoutineSafetyAuditsIncidentReportingItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RoutineSafetyAuditsIncidentReportingQueryResult>.Success(new RoutineSafetyAuditsIncidentReportingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RoutineSafetyAuditsIncidentReporting records for Tenant {TenantId}", request.TenantId);

            return Result<RoutineSafetyAuditsIncidentReportingQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.RoutineSafetyAuditsIncidentReporting.RetrievalFailed",
                "Unable to retrieve RoutineSafetyAuditsIncidentReporting records at this time.",
                Guid.NewGuid()));
        }
    }
}