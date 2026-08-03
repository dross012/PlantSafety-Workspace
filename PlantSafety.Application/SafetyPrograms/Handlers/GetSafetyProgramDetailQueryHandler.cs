// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetSafetyProgramDetailQuery. Loads the SafetyProgram aggregate
//   (its own native tracking data: name/description/status/dates/owner/participation/goals),
//   then fans out via ISender to a curated pair of representative read-model queries in the
//   sibling SafetyPolicies/SafetyProcedures/SafetyCulture modules -- one per Category -- to
//   compose a live "LinkedActivity" rollup. This is the sanctioned cross-module composition
//   pattern used elsewhere in the platform (see
//   PlantSafety.Application.Operations.SystemHealth.Handlers.GetSystemHealthReportQueryHandler
//   and PlantSafety.Application.Auditing.AuditReport.Commands.GenerateAuditReportCommandHandler):
//   dispatch sibling-module MediatR queries through ISender rather than reaching across module
//   boundaries via raw repository access, and degrade each section independently on failure
//   rather than failing the whole detail view.
//
//   Category -> sibling-query mapping (documented scoping assumption; see
//   docs/Phase5/ui-gaps-scoping.md section 8 -- the UI's original DTO comment names exactly
//   these four categories: Training, Behavioral, Compliance, Recognition):
//     Training    -> SafetyPolicies.SafetyTrainingReporting + SafetyCulture.TrainingCompletionRate
//     Behavioral  -> SafetyPolicies.SafetyCultureBehavioral + SafetyCulture.BehavioralObservationCompletion
//     Compliance  -> SafetyProcedures.RoutineSafetyAuditsIncidentReporting + SafetyCulture.NearMissReportingRate
//     Recognition -> SafetyCulture.SafetySuggestionSubmissions + SafetyCulture.LeadershipEngagementFrequency
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;

using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPrograms.Models;
using PlantSafety.Application.SafetyPrograms.Queries;

using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Queries;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Queries;
using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Queries;

namespace PlantSafety.Application.SafetyPrograms.Handlers;

/// <summary>Handles <see cref="GetSafetyProgramDetailQuery"/>.</summary>
public sealed class GetSafetyProgramDetailQueryHandler : IRequestHandler<GetSafetyProgramDetailQuery, Result<SafetyProgramDetailDto>>
{
    private const string StatusActive = "Active";

    private readonly ISafetyProgramRepository _repository;
    private readonly ISender _sender;
    private readonly ILogger<GetSafetyProgramDetailQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyProgramDetailQueryHandler(ISafetyProgramRepository repository, ISender sender, ILogger<GetSafetyProgramDetailQueryHandler> logger)
    {
        _repository = repository;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyProgramDetailDto>> Handle(GetSafetyProgramDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyProgramId.FromGuid(request.SafetyProgramId);
            var program = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);

            if (program is null)
            {
                return Result<SafetyProgramDetailDto>.Failure(new ApplicationError(
                    "SafetyPrograms.SafetyProgram.NotFound",
                    $"SafetyProgram '{request.SafetyProgramId}' was not found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var linkedActivity = await BuildLinkedActivityAsync(request.TenantId, program.SiteId, program.Category, cancellationToken);

            var dto = new SafetyProgramDetailDto
            {
                ProgramId = program.SafetyProgramId.Value,
                Name = program.Name,
                Description = program.Description,
                Category = program.Category.ToString(),
                Status = program.Status.ToString(),
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                ParticipationPercent = program.ParticipationPercent,
                EnrolledCount = program.EnrolledCount,
                // NOTE: no user-directory lookup service exists in this codebase yet; the
                // owner's GUID is surfaced as a display-name fallback. See
                // docs/Phase5/ui-gaps-scoping.md section 8.
                OwnerName = program.OwnerId.Value.ToString(),
                Goals = program.Goals.Select(g => g.Text).ToList(),
                LinkedActivity = linkedActivity,
            };

            return Result<SafetyProgramDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyProgram detail for {SafetyProgramId}", request.SafetyProgramId);

            return Result<SafetyProgramDetailDto>.Failure(new ApplicationError(
                "SafetyPrograms.SafetyProgram.DetailRetrievalFailed",
                "Unable to retrieve SafetyProgram detail at this time.",
                Guid.NewGuid()));
        }
    }

    /// <summary>
    /// Fans out via <see cref="ISender"/> to the curated pair of sibling-module read-model
    /// queries for the given program Category. Each section degrades independently -- a
    /// failure or unmapped category never fails the overall program detail.
    /// </summary>
    private async Task<IReadOnlyList<SafetyProgramLinkedActivityDto>> BuildLinkedActivityAsync(
        Guid tenantId, Guid? siteId, SafetyProgramCategory category, CancellationToken cancellationToken)
    {
        // A broad, fixed lookback window: this rollup answers "how much related activity
        // exists right now", not a historical report, so an exact program-date-aligned range
        // is unnecessary.
        var to = DateTime.UtcNow;
        var from = to.AddYears(-2);

        Task<SafetyProgramLinkedActivityDto>[] sections = category switch
        {
            SafetyProgramCategory.Training =>
            [
                BuildSectionAsync("SafetyPolicies", "SafetyTrainingReporting",
                    () => _sender.Send(new GetSafetyTrainingReportingQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
                BuildSectionAsync("SafetyCulture", "TrainingCompletionRate",
                    () => _sender.Send(new GetTrainingCompletionRateQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
            ],
            SafetyProgramCategory.Behavioral =>
            [
                BuildSectionAsync("SafetyPolicies", "SafetyCultureBehavioral",
                    () => _sender.Send(new GetSafetyCultureBehavioralQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
                BuildSectionAsync("SafetyCulture", "BehavioralObservationCompletion",
                    () => _sender.Send(new GetBehavioralObservationCompletionQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
            ],
            SafetyProgramCategory.Compliance =>
            [
                BuildSectionAsync("SafetyProcedures", "RoutineSafetyAuditsIncidentReporting",
                    () => _sender.Send(new GetRoutineSafetyAuditsIncidentReportingQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.AuditStatus == StatusActive)),
                BuildSectionAsync("SafetyCulture", "NearMissReportingRate",
                    () => _sender.Send(new GetNearMissReportingRateQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
            ],
            SafetyProgramCategory.Recognition =>
            [
                BuildSectionAsync("SafetyCulture", "SafetySuggestionSubmissions",
                    () => _sender.Send(new GetSafetySuggestionSubmissionsQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
                BuildSectionAsync("SafetyCulture", "LeadershipEngagementFrequency",
                    () => _sender.Send(new GetLeadershipEngagementFrequencyQuery(tenantId, siteId, from, to), cancellationToken),
                    r => r.Items.Count, r => r.Items.Count(i => i.Status == StatusActive)),
            ],
            _ => [],
        };

        return await Task.WhenAll(sections);
    }

    /// <summary>Runs a single sibling-module query, mapping its Result&lt;T&gt; outcome into a resilient linked-activity section.</summary>
    private async Task<SafetyProgramLinkedActivityDto> BuildSectionAsync<TResult>(
        string sourceModule,
        string sourceCategory,
        Func<Task<Result<TResult>>> query,
        Func<TResult, int> totalCount,
        Func<TResult, int> activeCount)
    {
        try
        {
            var result = await query();

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "SafetyProgram LinkedActivity: {SourceModule}.{SourceCategory} section unavailable: {Reason}",
                    sourceModule, sourceCategory, result.Error.Message);

                return new SafetyProgramLinkedActivityDto
                {
                    SourceModule = sourceModule,
                    SourceCategory = sourceCategory,
                    IsAvailable = false,
                    UnavailableReason = result.Error.Message,
                };
            }

            return new SafetyProgramLinkedActivityDto
            {
                SourceModule = sourceModule,
                SourceCategory = sourceCategory,
                ItemCount = totalCount(result.Value),
                ActiveItemCount = activeCount(result.Value),
                IsAvailable = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SafetyProgram LinkedActivity: {SourceModule}.{SourceCategory} section failed unexpectedly.", sourceModule, sourceCategory);

            return new SafetyProgramLinkedActivityDto
            {
                SourceModule = sourceModule,
                SourceCategory = sourceCategory,
                IsAvailable = false,
                UnavailableReason = $"{sourceModule}.{sourceCategory} is temporarily unavailable.",
            };
        }
    }
}
