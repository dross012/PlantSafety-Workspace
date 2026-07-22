// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSafetyMeetingParticipationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Handlers;

/// <summary>Handles <see cref="GetSafetyMeetingParticipationQuery"/>.</summary>
public sealed class GetSafetyMeetingParticipationQueryHandler : IRequestHandler<GetSafetyMeetingParticipationQuery, Result<SafetyMeetingParticipationQueryResult>>
{
    private readonly ISafetyMeetingParticipationRepository _repository;
    private readonly ILogger<GetSafetyMeetingParticipationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyMeetingParticipationQueryHandler(ISafetyMeetingParticipationRepository repository, ILogger<GetSafetyMeetingParticipationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyMeetingParticipationQueryResult>> Handle(GetSafetyMeetingParticipationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetyMeetingParticipationQueryResult>.Success(new SafetyMeetingParticipationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyMeetingParticipation for Tenant {TenantId}", request.TenantId);

            return Result<SafetyMeetingParticipationQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.SafetyMeetingParticipation.RetrievalFailed",
                "Unable to retrieve Employee Participation in Safety Meetings data at this time.",
                Guid.NewGuid()));
        }
    }
}