// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLeadingIndicatorsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scorecards.LeadingIndicators.Queries;
using PlantSafety.Application.Scorecards.LeadingIndicators.Repositories;

namespace PlantSafety.Application.Scorecards.LeadingIndicators.Handlers;

/// <summary>Handles <see cref="GetLeadingIndicatorsQuery"/>.</summary>
public sealed class GetLeadingIndicatorsQueryHandler : IRequestHandler<GetLeadingIndicatorsQuery, Result<LeadingIndicatorsQueryResult>>
{
    private readonly ILeadingIndicatorsRepository _repository;
    private readonly ILogger<GetLeadingIndicatorsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLeadingIndicatorsQueryHandler(ILeadingIndicatorsRepository repository, ILogger<GetLeadingIndicatorsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LeadingIndicatorsQueryResult>> Handle(GetLeadingIndicatorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLeadingIndicatorsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LeadingIndicatorsQueryResult>.Success(new LeadingIndicatorsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LeadingIndicators for Tenant {TenantId}", request.TenantId);

            return Result<LeadingIndicatorsQueryResult>.Failure(new ApplicationError(
                "Scorecards.LeadingIndicators.RetrievalFailed",
                "Unable to retrieve LeadingIndicators data at this time.",
                Guid.NewGuid()));
        }
    }
}