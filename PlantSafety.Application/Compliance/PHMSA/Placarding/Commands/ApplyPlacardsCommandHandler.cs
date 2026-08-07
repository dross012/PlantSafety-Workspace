// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ApplyPlacardsCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ApplyPlacardsCommand. Invokes PlacardingEngine.GeneratePlacards
//                  against the supplied load materials, then calls aggregate.ApplyPlacards(...)
//                  with the computed placard set. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Services;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Handles <see cref="ApplyPlacardsCommand"/>.</summary>
public sealed class ApplyPlacardsCommandHandler : IRequestHandler<ApplyPlacardsCommand, Result<HazmatContainerStatusResult>>
{
    private readonly IHazmatContainerRepository _repository;
    private readonly PlacardingEngine _placardingEngine;
    private readonly ILogger<ApplyPlacardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApplyPlacardsCommandHandler(
        IHazmatContainerRepository repository,
        PlacardingEngine placardingEngine,
        ILogger<ApplyPlacardsCommandHandler> logger)
    {
        _repository = repository;
        _placardingEngine = placardingEngine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatContainerStatusResult>> Handle(ApplyPlacardsCommand request, CancellationToken cancellationToken)
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

            var materials = new List<(HazardClass HazardClass, decimal Quantity, string QuantityUnit)>();
            foreach (var item in request.Materials ?? [])
            {
                if (!HazardClassParser.TryParse(item.HazardClass, out var hazardClass))
                {
                    return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                        "Compliance.PHMSA.Placarding.InvalidHazardClass",
                        $"'{item.HazardClass}' is not a recognized DOT hazard class.",
                        Guid.NewGuid(),
                        ErrorSeverity.Validation));
                }

                materials.Add((hazardClass, item.Quantity, item.QuantityUnit));
            }

            var placardingResult = _placardingEngine.GeneratePlacards(new PlacardingInput(materials, request.IsBulkPackaging));
            if (!placardingResult.Success)
            {
                return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Placarding.CalculationFailed",
                    string.Join(" ", placardingResult.Messages),
                    Guid.NewGuid(),
                    ErrorSeverity.Validation));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ApplyPlacards(
                placards: placardingResult.RequiredPlacards,
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
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply placards to HazmatContainer {HazmatContainerId}", request.HazmatContainerId);

            return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.ApplyPlacardsFailed",
                "Unable to apply placards at this time.",
                Guid.NewGuid()));
        }
    }
}
