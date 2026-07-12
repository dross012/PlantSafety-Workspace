// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEnvironmentalSamplingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalSamplingQuery"/>.</summary>
public sealed class GetEnvironmentalSamplingQueryHandler : IRequestHandler<GetEnvironmentalSamplingQuery, Result<EnvironmentalSamplingQueryResult>>
{
    private readonly IEnvironmentalSamplingRepository _repository;
    private readonly ILogger<GetEnvironmentalSamplingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalSamplingQueryHandler(IEnvironmentalSamplingRepository repository, ILogger<GetEnvironmentalSamplingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalSamplingQueryResult>> Handle(GetEnvironmentalSamplingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInspectionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EnvironmentalSamplingQueryResult>.Success(new EnvironmentalSamplingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnvironmentalSampling for Tenant {TenantId}", request.TenantId);

            return Result<EnvironmentalSamplingQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.RetrievalFailed",
                "Unable to retrieve EnvironmentalSampling data at this time.",
                Guid.NewGuid()));
        }
    }
}
