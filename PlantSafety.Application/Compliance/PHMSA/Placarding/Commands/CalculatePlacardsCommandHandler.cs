// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           CalculatePlacardsCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for CalculatePlacardsCommand. Wraps PlacardingEngine.GeneratePlacards
//                  directly; performs no persistence.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Handles <see cref="CalculatePlacardsCommand"/>.</summary>
public sealed class CalculatePlacardsCommandHandler : IRequestHandler<CalculatePlacardsCommand, Result<CalculatePlacardsResult>>
{
    private readonly PlacardingEngine _placardingEngine;
    private readonly ILogger<CalculatePlacardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CalculatePlacardsCommandHandler(PlacardingEngine placardingEngine, ILogger<CalculatePlacardsCommandHandler> logger)
    {
        _placardingEngine = placardingEngine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<CalculatePlacardsResult>> Handle(CalculatePlacardsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var materials = new List<(HazardClass HazardClass, decimal Quantity, string QuantityUnit)>();
            foreach (var item in request.Materials ?? [])
            {
                if (!HazardClassParser.TryParse(item.HazardClass, out var hazardClass))
                {
                    return Task.FromResult(Result<CalculatePlacardsResult>.Failure(new ApplicationError(
                        "Compliance.PHMSA.Placarding.InvalidHazardClass",
                        $"'{item.HazardClass}' is not a recognized DOT hazard class.",
                        Guid.NewGuid(),
                        ErrorSeverity.Validation)));
                }

                materials.Add((hazardClass, item.Quantity, item.QuantityUnit));
            }

            var placardingResult = _placardingEngine.GeneratePlacards(new PlacardingInput(materials, request.IsBulkPackaging));

            return Task.FromResult(Result<CalculatePlacardsResult>.Success(new CalculatePlacardsResult(
                placardingResult.Success,
                placardingResult.RequiredPlacards,
                placardingResult.AnyOrientationArrowsRequired,
                placardingResult.Messages)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate placards for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<CalculatePlacardsResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.CalculationFailed",
                "Unable to calculate placards at this time.",
                Guid.NewGuid())));
        }
    }
}
