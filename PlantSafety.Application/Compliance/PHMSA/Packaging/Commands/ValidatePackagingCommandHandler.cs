// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           ValidatePackagingCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ValidatePackagingCommand. Invokes the Domain-layer PackagingEngine
//                  (instantiated directly, mirroring the existing PermitRequiresTrainingRule
//                  direct-instantiation precedent for stateless domain services) and returns its
//                  result. No persistence, no repository dependency.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Services;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Handles <see cref="ValidatePackagingCommand"/>.</summary>
public sealed class ValidatePackagingCommandHandler : IRequestHandler<ValidatePackagingCommand, Result<ValidatePackagingResult>>
{
    private static readonly PackagingEngine Engine = new();

    private readonly ILogger<ValidatePackagingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ValidatePackagingCommandHandler(ILogger<ValidatePackagingCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<ValidatePackagingResult>> Handle(ValidatePackagingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<PackingGroup>(request.PackingGroup, ignoreCase: true, out var packingGroup))
            {
                return Task.FromResult(Result<ValidatePackagingResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Packaging.InvalidPackingGroup",
                    $"'{request.PackingGroup}' is not a recognized DOT packing group.",
                    Guid.NewGuid())));
            }

            var dotSpec = new DOTSpec(request.ProposedDOTSpec);
            var hazardClass = new HazardClass(request.HazardClass, request.HazardClassDivision);

            var validation = Engine.ValidatePackaging(dotSpec, hazardClass, packingGroup);

            return Task.FromResult(Result<ValidatePackagingResult>.Success(new ValidatePackagingResult(
                validation.IsAuthorized,
                validation.Message)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate packaging spec '{ProposedDOTSpec}' for Tenant {TenantId}", request.ProposedDOTSpec, request.TenantId);

            return Task.FromResult(Result<ValidatePackagingResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Packaging.ValidationFailed",
                "Unable to validate the packaging specification at this time.",
                Guid.NewGuid())));
        }
    }
}
