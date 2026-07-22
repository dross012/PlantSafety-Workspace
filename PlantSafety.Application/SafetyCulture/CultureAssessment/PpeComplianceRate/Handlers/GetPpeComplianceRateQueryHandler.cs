// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPpeComplianceRateQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Handlers;

/// <summary>Handles <see cref="GetPpeComplianceRateQuery"/>.</summary>
public sealed class GetPpeComplianceRateQueryHandler : IRequestHandler<GetPpeComplianceRateQuery, Result<PpeComplianceRateQueryResult>>
{
    private readonly IPpeComplianceRateRepository _repository;
    private readonly ILogger<GetPpeComplianceRateQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPpeComplianceRateQueryHandler(IPpeComplianceRateRepository repository, ILogger<GetPpeComplianceRateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpeComplianceRateQueryResult>> Handle(GetPpeComplianceRateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PpeComplianceRateQueryResult>.Success(new PpeComplianceRateQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PpeComplianceRate for Tenant {TenantId}", request.TenantId);

            return Result<PpeComplianceRateQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.PpeComplianceRate.RetrievalFailed",
                "Unable to retrieve PPE Compliance Rate data at this time.",
                Guid.NewGuid()));
        }
    }
}