// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           SuggestPackagingQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for SuggestPackagingQuery. Invokes the Domain-layer PackagingEngine
//                  (instantiated directly, mirroring the existing PermitRequiresTrainingRule
//                  direct-instantiation precedent for stateless domain services). No persistence,
//                  no repository dependency.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Handlers;

/// <summary>Handles <see cref="Queries.SuggestPackagingQuery"/>.</summary>
public sealed class SuggestPackagingQueryHandler : IRequestHandler<Queries.SuggestPackagingQuery, Result<Queries.SuggestPackagingResult>>
{
    private static readonly PackagingEngine Engine = new();

    private readonly ILogger<SuggestPackagingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuggestPackagingQueryHandler(ILogger<SuggestPackagingQueryHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<Queries.SuggestPackagingResult>> Handle(Queries.SuggestPackagingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<PackingGroup>(request.PackingGroup, ignoreCase: true, out var packingGroup))
            {
                return Task.FromResult(Result<Queries.SuggestPackagingResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Packaging.InvalidPackingGroup",
                    $"'{request.PackingGroup}' is not a recognized DOT packing group.",
                    Guid.NewGuid())));
            }

            var hazardClass = new HazardClass(request.HazardClass, request.HazardClassDivision);

            var determination = Engine.DeterminePackaging(new PackagingDeterminationInput(
                HazardClass: hazardClass,
                PackingGroup: packingGroup,
                Quantity: request.Quantity,
                QuantityUnit: request.QuantityUnit,
                ContainerTypeRequested: request.ContainerTypeRequested));

            if (!determination.Success)
            {
                return Task.FromResult(Result<Queries.SuggestPackagingResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Packaging.DeterminationFailed",
                    string.Join(" ", determination.Messages),
                    Guid.NewGuid())));
            }

            return Task.FromResult(Result<Queries.SuggestPackagingResult>.Success(new Queries.SuggestPackagingResult(
                determination.AuthorizedPackaging.Select(spec => spec.Value).ToList(),
                determination.ClosureInstructions,
                determination.Messages)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine packaging suggestion for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<Queries.SuggestPackagingResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Packaging.DeterminationFailed",
                "Unable to determine packaging at this time.",
                Guid.NewGuid())));
        }
    }
}
