// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetComplianceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Compliance.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Compliance.Repositories;

namespace PlantSafety.Application.Analytics.ScoreCards.Compliance.Handlers;

/// <summary>Handles <see cref="GetComplianceQuery"/>.</summary>
public sealed class GetComplianceQueryHandler : IRequestHandler<GetComplianceQuery, Result<ComplianceQueryResult>>
{
    private readonly IComplianceRepository _repository;
    private readonly ILogger<GetComplianceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetComplianceQueryHandler(IComplianceRepository repository, ILogger<GetComplianceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceQueryResult>> Handle(GetComplianceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetComplianceScoresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ComplianceQueryResult>.Success(new ComplianceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Compliance for Tenant {TenantId}", request.TenantId);

            return Result<ComplianceQueryResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.Compliance.RetrievalFailed",
                "Unable to retrieve Compliance data at this time.",
                Guid.NewGuid()));
        }
    }
}
