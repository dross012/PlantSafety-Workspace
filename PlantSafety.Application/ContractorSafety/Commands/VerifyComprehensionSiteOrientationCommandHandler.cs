// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for VerifyComprehensionSiteOrientationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorSafety.SiteOrientation.Interfaces;
using PlantSafety.Domain.ContractorSafety.SiteOrientation.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Handles <see cref="VerifyComprehensionSiteOrientationCommand"/>.</summary>
public sealed class VerifyComprehensionSiteOrientationCommandHandler : IRequestHandler<VerifyComprehensionSiteOrientationCommand, Result<SiteOrientationActionResult>>
{
    private readonly ISiteOrientationRepository _repository;
    private readonly ILogger<VerifyComprehensionSiteOrientationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VerifyComprehensionSiteOrientationCommandHandler(ISiteOrientationRepository repository, ILogger<VerifyComprehensionSiteOrientationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SiteOrientationActionResult>> Handle(VerifyComprehensionSiteOrientationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SiteOrientationId.FromGuid(request.SiteOrientationId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<SiteOrientationActionResult>.Failure(new ApplicationError(
                    "ContractorSafety.SiteOrientation.NotFound",
                    "The requested SiteOrientation record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.VerifyComprehension(
                request.ComprehensionScore,
                UtcTimestamp.Now(),
                request.ValidityDays,
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SiteOrientationActionResult>.Success(new SiteOrientationActionResult(
                aggregate.SiteOrientationId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute VerifyComprehension for SiteOrientation {SiteOrientationId}", request.SiteOrientationId);

            return Result<SiteOrientationActionResult>.Failure(new ApplicationError(
                "ContractorSafety.SiteOrientation.VerifyComprehensionFailed",
                "Unable to VerifyComprehension the SiteOrientation record at this time.",
                Guid.NewGuid()));
        }
    }
}