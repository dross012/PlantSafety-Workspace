// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetInvestigationRootCauseAnalysisQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Queries;
using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Repositories;

namespace PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Handlers;

/// <summary>Handles <see cref="GetInvestigationRootCauseAnalysisQuery"/>.</summary>
public sealed class GetInvestigationRootCauseAnalysisQueryHandler : IRequestHandler<GetInvestigationRootCauseAnalysisQuery, Result<InvestigationRootCauseAnalysisQueryResult>>
{
    private readonly IInvestigationRootCauseAnalysisRepository _repository;
    private readonly ILogger<GetInvestigationRootCauseAnalysisQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetInvestigationRootCauseAnalysisQueryHandler(IInvestigationRootCauseAnalysisRepository repository, ILogger<GetInvestigationRootCauseAnalysisQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<InvestigationRootCauseAnalysisQueryResult>> Handle(GetInvestigationRootCauseAnalysisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInvestigationRootCauseAnalysisAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<InvestigationRootCauseAnalysisQueryResult>.Success(new InvestigationRootCauseAnalysisQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve InvestigationRootCauseAnalysis for Tenant {TenantId}", request.TenantId);

            return Result<InvestigationRootCauseAnalysisQueryResult>.Failure(new ApplicationError(
                "AccidentManagement.InvestigationRootCauseAnalysis.RetrievalFailed",
                "Unable to retrieve InvestigationRootCauseAnalysis data at this time.",
                Guid.NewGuid()));
        }
    }
}