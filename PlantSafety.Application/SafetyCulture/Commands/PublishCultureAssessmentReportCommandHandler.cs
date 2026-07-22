// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for PublishCultureAssessmentReportCommand. Pure orchestration: loads the CultureAssessment aggregate, invokes its PublishReport() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Handles <see cref="PublishCultureAssessmentReportCommand"/>.</summary>
public sealed class PublishCultureAssessmentReportCommandHandler : IRequestHandler<PublishCultureAssessmentReportCommand, Result<PublishCultureAssessmentReportResult>>
{
    private readonly ICultureAssessmentRepository _repository;
    private readonly ILogger<PublishCultureAssessmentReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishCultureAssessmentReportCommandHandler(ICultureAssessmentRepository repository, ILogger<PublishCultureAssessmentReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PublishCultureAssessmentReportResult>> Handle(PublishCultureAssessmentReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CultureAssessmentId.FromGuid(request.CultureAssessmentId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PublishCultureAssessmentReportResult>.Failure(new ApplicationError(
                    "SafetyCulture.CultureAssessment.NotFound",
                    "The requested CultureAssessment record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.PublishReport(
                summary: request.Summary,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PublishCultureAssessmentReportResult>.Success(new PublishCultureAssessmentReportResult(
                aggregate.CultureAssessmentId.Value,
                aggregate.Status.ToString(),
                aggregate.OverallCultureScore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish report for CultureAssessment {CultureAssessmentId}", request.CultureAssessmentId);

            return Result<PublishCultureAssessmentReportResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.PublishReportFailed",
                "Unable to publish the CultureAssessment report at this time.",
                request.CorrelationId));
        }
    }
}
