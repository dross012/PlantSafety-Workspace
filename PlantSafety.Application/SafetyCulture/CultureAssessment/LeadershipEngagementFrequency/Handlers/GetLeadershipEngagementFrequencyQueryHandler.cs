// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLeadershipEngagementFrequencyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Handlers;

/// <summary>Handles <see cref="GetLeadershipEngagementFrequencyQuery"/>.</summary>
public sealed class GetLeadershipEngagementFrequencyQueryHandler : IRequestHandler<GetLeadershipEngagementFrequencyQuery, Result<LeadershipEngagementFrequencyQueryResult>>
{
    private readonly ILeadershipEngagementFrequencyRepository _repository;
    private readonly ILogger<GetLeadershipEngagementFrequencyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLeadershipEngagementFrequencyQueryHandler(ILeadershipEngagementFrequencyRepository repository, ILogger<GetLeadershipEngagementFrequencyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LeadershipEngagementFrequencyQueryResult>> Handle(GetLeadershipEngagementFrequencyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LeadershipEngagementFrequencyQueryResult>.Success(new LeadershipEngagementFrequencyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LeadershipEngagementFrequency for Tenant {TenantId}", request.TenantId);

            return Result<LeadershipEngagementFrequencyQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.RetrievalFailed",
                "Unable to retrieve Leadership Engagement Frequency data at this time.",
                Guid.NewGuid()));
        }
    }
}