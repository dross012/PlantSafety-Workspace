// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetAutomatedIncidentDetectionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.AutomatedIncidentDetection.Queries;
using PlantSafety.Application.Observability.AutomatedIncidentDetection.Repositories;

namespace PlantSafety.Application.Observability.AutomatedIncidentDetection.Handlers;

/// <summary>Handles <see cref="GetAutomatedIncidentDetectionQuery"/>.</summary>
public sealed class GetAutomatedIncidentDetectionQueryHandler : IRequestHandler<GetAutomatedIncidentDetectionQuery, Result<AutomatedIncidentDetectionQueryResult>>
{
    private readonly IAutomatedIncidentDetectionRepository _repository;
    private readonly ILogger<GetAutomatedIncidentDetectionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAutomatedIncidentDetectionQueryHandler(IAutomatedIncidentDetectionRepository repository, ILogger<GetAutomatedIncidentDetectionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutomatedIncidentDetectionQueryResult>> Handle(GetAutomatedIncidentDetectionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAutomatedIncidentDetectionAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AutomatedIncidentDetectionQueryResult>.Success(new AutomatedIncidentDetectionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AutomatedIncidentDetection for Tenant {TenantId}", request.TenantId);

            return Result<AutomatedIncidentDetectionQueryResult>.Failure(new ApplicationError(
                "Observability.AutomatedIncidentDetection.RetrievalFailed",
                "Unable to retrieve AutomatedIncidentDetection data at this time.",
                Guid.NewGuid()));
        }
    }
}