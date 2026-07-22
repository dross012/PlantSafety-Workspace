// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSafetySuggestionSubmissionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Handlers;

/// <summary>Handles <see cref="GetSafetySuggestionSubmissionsQuery"/>.</summary>
public sealed class GetSafetySuggestionSubmissionsQueryHandler : IRequestHandler<GetSafetySuggestionSubmissionsQuery, Result<SafetySuggestionSubmissionsQueryResult>>
{
    private readonly ISafetySuggestionSubmissionsRepository _repository;
    private readonly ILogger<GetSafetySuggestionSubmissionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetySuggestionSubmissionsQueryHandler(ISafetySuggestionSubmissionsRepository repository, ILogger<GetSafetySuggestionSubmissionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetySuggestionSubmissionsQueryResult>> Handle(GetSafetySuggestionSubmissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetySuggestionSubmissionsQueryResult>.Success(new SafetySuggestionSubmissionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetySuggestionSubmissions for Tenant {TenantId}", request.TenantId);

            return Result<SafetySuggestionSubmissionsQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.RetrievalFailed",
                "Unable to retrieve Safety Suggestion Submissions data at this time.",
                Guid.NewGuid()));
        }
    }
}