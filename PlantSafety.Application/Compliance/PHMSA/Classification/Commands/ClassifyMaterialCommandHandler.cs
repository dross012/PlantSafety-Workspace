// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ClassifyMaterialCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ClassifyMaterialCommand. Runs the raw candidate-material input
//                  through the Domain-layer ClassificationEngine (a pure, stateless, deterministic
//                  domain service -- instantiated directly, mirroring the existing
//                  PermitRequiresTrainingRule direct-instantiation precedent rather than
//                  registering a stateless domain service in DI); when the engine cannot classify
//                  the material it returns Result.Failure with the engine's explanatory messages
//                  rather than fabricating a classification. On success, constructs the
//                  HazmatMaterial aggregate via its domain factory method and persists it through
//                  the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Commands;

/// <summary>Handles <see cref="ClassifyMaterialCommand"/>.</summary>
public sealed class ClassifyMaterialCommandHandler : IRequestHandler<ClassifyMaterialCommand, Result<ClassifyMaterialResult>>
{
    private static readonly ClassificationEngine Engine = new();

    private readonly IHazmatMaterialRepository _repository;
    private readonly ILogger<ClassifyMaterialCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ClassifyMaterialCommandHandler(IHazmatMaterialRepository repository, ILogger<ClassifyMaterialCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ClassifyMaterialResult>> Handle(ClassifyMaterialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var classification = Engine.ClassifyMaterial(new ClassificationInput(
                CasNumber: request.CasNumber,
                WasteProfileCode: request.WasteProfileCode,
                SdsHazardStatement: request.SdsHazardStatement,
                MaterialName: request.MaterialName));

            if (!classification.Success)
            {
                return Result<ClassifyMaterialResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Classification.ClassificationFailed",
                    string.Join(" ", classification.Messages),
                    Guid.NewGuid()));
            }

            var classifiedBy = UserId.FromGuid(request.ClassifiedByUserId);

            var aggregate = PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates.HazmatMaterial.Create(
                unNumber: classification.UNNumber!.Value,
                shippingName: classification.ProperShippingName!.Value,
                hazardClass: classification.HazardClass!.Value,
                packingGroup: classification.PackingGroup!.Value,
                specialProvisions: classification.SpecialProvisions,
                ergGuideNumber: classification.ErgGuideNumber,
                isMarinePollutant: classification.IsMarinePollutant,
                isTIH: classification.IsTIH,
                tihZone: classification.TIHZone,
                createdBy: classifiedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(classifiedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<ClassifyMaterialResult>.Success(new ClassifyMaterialResult(
                aggregate.Id.Value,
                aggregate.UNNumber.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to classify hazardous material '{MaterialName}' for Tenant {TenantId}", request.MaterialName, request.TenantId);

            return Result<ClassifyMaterialResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Classification.ClassifyFailed",
                "Unable to classify the material at this time.",
                Guid.NewGuid()));
        }
    }
}
