// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.CraneAndHoist
//  Layer:          Application
//  File:           EvaluateCraneComplianceCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateCraneComplianceCommand. Loads real
//                  PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems
//                  aggregate data for the tenant/site via ICraneHoistSystemsRepository, projects
//                  each aggregate into the Domain engine's CraneHoistAssetSnapshot record, invokes
//                  CraneAndHoistComplianceEngine.EvaluateCompliance(...), and translates the
//                  Domain-layer Result&lt;StandardsComplianceReport&gt; into the Application-layer
//                  Result&lt;StandardsComplianceReportDto&gt;. Mirrors
//                  Compliance.EPA.Compliance.Commands.EvaluateRcraComplianceCommandHandler's
//                  engine-invocation style. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Standards.CraneAndHoist.Services;
using PlantSafety.Domain.Compliance.Standards.CraneAndHoist.ValueObjects;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;
using PlantSafety.Application.Compliance.Standards.Compliance.Specifications;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateCraneComplianceCommand"/>.</summary>
public sealed class EvaluateCraneComplianceCommandHandler : IRequestHandler<EvaluateCraneComplianceCommand, Result<StandardsComplianceReportDto>>
{
    private static readonly CraneAndHoistComplianceEngine Engine = new();

    private readonly ICraneHoistSystemsRepository _craneHoistSystemsRepository;
    private readonly ILogger<EvaluateCraneComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateCraneComplianceCommandHandler(
        ICraneHoistSystemsRepository craneHoistSystemsRepository,
        ILogger<EvaluateCraneComplianceCommandHandler> logger)
    {
        _craneHoistSystemsRepository = craneHoistSystemsRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StandardsComplianceReportDto>> Handle(EvaluateCraneComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var specification = new CraneHoistSystemsTenantSiteSpecification(request.TenantId, request.SiteId);
            var assets = await _craneHoistSystemsRepository.ListAsync(specification, cancellationToken);

            var snapshots = assets.Select(a => new CraneHoistAssetSnapshot(
                AssetTag: a.AssetTag,
                AssetTitle: a.AssetTitle,
                AssetType: a.AssetType.ToString(),
                RatedLoadCapacityKg: a.RatedLoadCapacityKg,
                LastObservedLoadKg: a.LastObservedLoadKg,
                LastLiftAtUtc: a.LastLiftAtUtc,
                OverloadEventCount: a.OverloadEventCount,
                OverloadFlagged: a.OverloadFlagged,
                InspectionDueAtUtc: a.InspectionDueAtUtc,
                LastInspectionAtUtc: a.LastInspectionAtUtc,
                LastInspectionPassed: a.LastInspectionPassed,
                OperatorQualifiedAtUtc: a.OperatorQualifiedAtUtc,
                OperatorQualificationExpiresAtUtc: a.OperatorQualificationExpiresAtUtc,
                OperatorQualificationSuspended: a.OperatorQualificationSuspended,
                LastRiggingInspectionAtUtc: a.LastRiggingInspectionAtUtc,
                LastRiggingDefectDetected: a.LastRiggingDefectDetected ?? false,
                Status: a.Status.ToString())).ToList();

            var input = new CraneAndHoistComplianceInput(
                Assets: snapshots,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.Standards.CraneComplianceEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate ASME B30 compliance for the requested tenant/site.",
                    Guid.NewGuid()));
            }

            return Result<StandardsComplianceReportDto>.Success(StandardsComplianceReportMapper.ToDto(evaluation.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate ASME B30 crane/hoist compliance for Tenant {TenantId}, Site {SiteId}", request.TenantId, request.SiteId);

            return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.Standards.CraneComplianceEvaluationFailed",
                "Unable to evaluate ASME B30 crane/hoist compliance at this time.",
                Guid.NewGuid()));
        }
    }
}
