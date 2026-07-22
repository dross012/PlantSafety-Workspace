// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateCultureAssessmentCommand. Pure orchestration: constructs the CultureAssessment aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Handles <see cref="CreateCultureAssessmentCommand"/>.</summary>
public sealed class CreateCultureAssessmentCommandHandler : IRequestHandler<CreateCultureAssessmentCommand, Result<CreateCultureAssessmentResult>>
{
    private readonly ICultureAssessmentRepository _repository;
    private readonly ILogger<CreateCultureAssessmentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateCultureAssessmentCommandHandler(ICultureAssessmentRepository repository, ILogger<CreateCultureAssessmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateCultureAssessmentResult>> Handle(CreateCultureAssessmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);


            var aggregate = PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                assessmentTitle: request.AssessmentTitle,
                periodStartUtc: UtcTimestamp.From(request.PeriodStartUtc),
                periodEndUtc: UtcTimestamp.From(request.PeriodEndUtc),
                summary: request.Summary);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateCultureAssessmentResult>.Success(new CreateCultureAssessmentResult(
                aggregate.CultureAssessmentId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create CultureAssessment for Tenant {TenantId}", request.TenantId);

            return Result<CreateCultureAssessmentResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.CreateFailed",
                "Unable to create the CultureAssessment record at this time.",
                Guid.NewGuid()));
        }
    }
}