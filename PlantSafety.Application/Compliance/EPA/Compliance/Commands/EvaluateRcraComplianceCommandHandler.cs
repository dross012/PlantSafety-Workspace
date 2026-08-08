// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.Rcra
//  Layer:          Application
//  File:           EvaluateRcraComplianceCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateRcraComplianceCommand. Loads real WasteContainer data
//                  (per the caller-supplied AccumulationAreaIds -- see command header for why) via
//                  IWasteContainerRepository, and real open-manifest data for the site via
//                  IHazardousWasteManifestRepository, projects both into the Domain engine's
//                  RcraContainerSnapshot/RcraManifestSnapshot records, invokes
//                  RcraWasteComplianceEngine.EvaluateCompliance(...), and translates the
//                  Domain-layer Result&lt;EpaComplianceReport&gt; into the Application-layer
//                  Result&lt;EpaComplianceReportDto&gt;. Mirrors
//                  PlantSafety.Application.Compliance.PHMSA.Classification.Commands
//                  .ClassifyMaterialCommandHandler's engine-invocation style. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.EPA.Rcra.Services;
using PlantSafety.Domain.Compliance.EPA.Rcra.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateRcraComplianceCommand"/>.</summary>
public sealed class EvaluateRcraComplianceCommandHandler : IRequestHandler<EvaluateRcraComplianceCommand, Result<EpaComplianceReportDto>>
{
    private static readonly RcraWasteComplianceEngine Engine = new();

    private readonly IWasteContainerRepository _wasteContainerRepository;
    private readonly IHazardousWasteManifestRepository _manifestRepository;
    private readonly ILogger<EvaluateRcraComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateRcraComplianceCommandHandler(
        IWasteContainerRepository wasteContainerRepository,
        IHazardousWasteManifestRepository manifestRepository,
        ILogger<EvaluateRcraComplianceCommandHandler> logger)
    {
        _wasteContainerRepository = wasteContainerRepository;
        _manifestRepository = manifestRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EpaComplianceReportDto>> Handle(EvaluateRcraComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var containers = new List<RcraContainerSnapshot>();
            foreach (var accumulationAreaId in request.AccumulationAreaIds)
            {
                var areaContainers = await _wasteContainerRepository.ListContainersByAccumulationAreaAsync(
                    request.TenantId, accumulationAreaId, cancellationToken);

                containers.AddRange(areaContainers.Select(c => new RcraContainerSnapshot(
                    WasteContainerId: c.WasteContainerId,
                    ContainerLabel: c.ContainerLabel,
                    RegulatoryTimeLimitDays: c.RegulatoryTimeLimitDays,
                    OpenedAtUtc: UtcTimestamp.From(c.OpenedAtUtc),
                    ClosedAtUtc: c.ClosedAtUtc.HasValue ? UtcTimestamp.From(c.ClosedAtUtc.Value) : null,
                    Status: c.Status)));
            }

            var openManifests = await _manifestRepository.ListOpenManifestsAsync(request.TenantId, request.SiteId, cancellationToken);

            var manifests = openManifests.Select(m => new RcraManifestSnapshot(
                ManifestId: m.HazardousWasteManifestId,
                ManifestNumber: m.ManifestNumber,
                GeneratedAtUtc: UtcTimestamp.From(m.GeneratedAtUtc),
                TransporterSignedAtUtc: m.TransporterSignedAtUtc.HasValue ? UtcTimestamp.From(m.TransporterSignedAtUtc.Value) : null,
                TsdfReceivedAtUtc: m.TsdfReceivedAtUtc.HasValue ? UtcTimestamp.From(m.TsdfReceivedAtUtc.Value) : null,
                ClosedAtUtc: m.ClosedAtUtc.HasValue ? UtcTimestamp.From(m.ClosedAtUtc.Value) : null,
                IsImmutable: m.IsImmutable,
                DiscrepancyReason: m.DiscrepancyReason,
                RetentionExpiresAtUtc: UtcTimestamp.From(m.RetentionExpiresAtUtc),
                Status: m.Status)).ToList();

            var input = new RcraComplianceInput(
                MonthlyHazardousWasteQuantityKg: request.MonthlyHazardousWasteQuantityKg,
                MonthlyAcuteHazardousWasteQuantityKg: request.MonthlyAcuteHazardousWasteQuantityKg,
                Containers: containers,
                Manifests: manifests,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.EPA.RcraEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate RCRA compliance for the requested site.",
                    Guid.NewGuid()));
            }

            return Result<EpaComplianceReportDto>.Success(EpaComplianceReportMapper.ToDto(evaluation.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate RCRA compliance for Site {SiteId}", request.SiteId);

            return Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.EPA.RcraEvaluationFailed",
                "Unable to evaluate RCRA compliance at this time.",
                Guid.NewGuid()));
        }
    }
}
