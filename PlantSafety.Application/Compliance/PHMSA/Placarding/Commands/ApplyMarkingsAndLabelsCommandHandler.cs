// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ApplyMarkingsAndLabelsCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ApplyMarkingsAndLabelsCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Handles <see cref="ApplyMarkingsAndLabelsCommand"/>.</summary>
public sealed class ApplyMarkingsAndLabelsCommandHandler : IRequestHandler<ApplyMarkingsAndLabelsCommand, Result<HazmatContainerStatusResult>>
{
    private readonly IHazmatContainerRepository _repository;
    private readonly ILogger<ApplyMarkingsAndLabelsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApplyMarkingsAndLabelsCommandHandler(IHazmatContainerRepository repository, ILogger<ApplyMarkingsAndLabelsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatContainerStatusResult>> Handle(ApplyMarkingsAndLabelsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazmatContainerId.FromGuid(request.HazmatContainerId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Placarding.NotFound",
                    "The requested hazmat container could not be found.",
                    request.HazmatContainerId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ApplyMarkingsAndLabels(
                markings: request.Markings,
                labels: request.Labels,
                orientationArrowsRequired: request.OrientationArrowsRequired,
                compatibilityGroup: request.CompatibilityGroup,
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<HazmatContainerStatusResult>.Success(new HazmatContainerStatusResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply markings/labels to HazmatContainer {HazmatContainerId}", request.HazmatContainerId);

            return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.ApplyMarkingsAndLabelsFailed",
                "Unable to apply markings and labels at this time.",
                Guid.NewGuid()));
        }
    }
}
