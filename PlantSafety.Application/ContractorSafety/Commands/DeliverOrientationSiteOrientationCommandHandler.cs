// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for DeliverOrientationSiteOrientationCommand.
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

/// <summary>Handles <see cref="DeliverOrientationSiteOrientationCommand"/>.</summary>
public sealed class DeliverOrientationSiteOrientationCommandHandler : IRequestHandler<DeliverOrientationSiteOrientationCommand, Result<SiteOrientationActionResult>>
{
    private readonly ISiteOrientationRepository _repository;
    private readonly ILogger<DeliverOrientationSiteOrientationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeliverOrientationSiteOrientationCommandHandler(ISiteOrientationRepository repository, ILogger<DeliverOrientationSiteOrientationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SiteOrientationActionResult>> Handle(DeliverOrientationSiteOrientationCommand request, CancellationToken cancellationToken)
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

            aggregate.DeliverOrientation(
                request.AttendeeCount,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SiteOrientationActionResult>.Success(new SiteOrientationActionResult(
                aggregate.SiteOrientationId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute DeliverOrientation for SiteOrientation {SiteOrientationId}", request.SiteOrientationId);

            return Result<SiteOrientationActionResult>.Failure(new ApplicationError(
                "ContractorSafety.SiteOrientation.DeliverOrientationFailed",
                "Unable to DeliverOrientation the SiteOrientation record at this time.",
                Guid.NewGuid()));
        }
    }
}