// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.ControlSystemSafety
//  Layer:          Application
//  File:           EvaluateControlSystemSafetyCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateControlSystemSafetyCommand. Loads real
//                  SafetyValvesRelays and PresenceSensingDevices aggregate data for the
//                  tenant/site via ISafetyValvesRelaysRepository/IPresenceSensingDevicesRepository,
//                  joins each real component against the caller-supplied classification
//                  (matched case-insensitively by ComponentTitle/DeviceTitle -- unmatched
//                  components default to RequiredPerformanceLevel.PLa with no achieved rating, see
//                  command header), builds the Domain engine's SafetyComponentSnapshot list,
//                  invokes ControlSystemSafetyEngine.EvaluateCompliance(...), and translates the
//                  Domain-layer Result&lt;StandardsComplianceReport&gt; into the Application-layer
//                  Result&lt;StandardsComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Enums;
using PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Services;
using PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Interfaces;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;
using PlantSafety.Application.Compliance.Standards.Compliance.Specifications;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateControlSystemSafetyCommand"/>.</summary>
public sealed class EvaluateControlSystemSafetyCommandHandler : IRequestHandler<EvaluateControlSystemSafetyCommand, Result<StandardsComplianceReportDto>>
{
    private static readonly ControlSystemSafetyEngine Engine = new();

    private readonly ISafetyValvesRelaysRepository _safetyValvesRelaysRepository;
    private readonly IPresenceSensingDevicesRepository _presenceSensingDevicesRepository;
    private readonly ILogger<EvaluateControlSystemSafetyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateControlSystemSafetyCommandHandler(
        ISafetyValvesRelaysRepository safetyValvesRelaysRepository,
        IPresenceSensingDevicesRepository presenceSensingDevicesRepository,
        ILogger<EvaluateControlSystemSafetyCommandHandler> logger)
    {
        _safetyValvesRelaysRepository = safetyValvesRelaysRepository;
        _presenceSensingDevicesRepository = presenceSensingDevicesRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StandardsComplianceReportDto>> Handle(EvaluateControlSystemSafetyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var classificationsByIdentifier = request.Classifications
                .GroupBy(c => c.ComponentIdentifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var valveSpecification = new SafetyValvesRelaysTenantSiteSpecification(request.TenantId, request.SiteId);
            var valves = await _safetyValvesRelaysRepository.ListAsync(valveSpecification, cancellationToken);

            var presenceSpecification = new PresenceSensingDevicesTenantSiteSpecification(request.TenantId, request.SiteId);
            var presenceDevices = await _presenceSensingDevicesRepository.ListAsync(presenceSpecification, cancellationToken);

            var snapshots = new List<SafetyComponentSnapshot>();

            foreach (var valve in valves)
            {
                var classification = ResolveClassification(valve.ComponentTitle, classificationsByIdentifier);

                snapshots.Add(new SafetyComponentSnapshot(
                    ComponentTitle: valve.ComponentTitle,
                    ComponentType: valve.ComponentType.ToString(),
                    EquipmentLocation: valve.EquipmentLocation,
                    RequiredPerformanceLevel: classification.RequiredPerformanceLevel,
                    AchievedPerformanceLevel: classification.AchievedPerformanceLevel,
                    Category: classification.Category,
                    InspectedAtUtc: valve.InspectedAtUtc,
                    NextInspectionDueAtUtc: valve.NextInspectionDueAtUtc,
                    FaultCount: valve.FaultCount,
                    ReliabilityOrAccuracyScore: valve.ReliabilityScore));
            }

            foreach (var device in presenceDevices)
            {
                var classification = ResolveClassification(device.DeviceTitle, classificationsByIdentifier);

                snapshots.Add(new SafetyComponentSnapshot(
                    ComponentTitle: device.DeviceTitle,
                    ComponentType: device.DeviceType.ToString(),
                    EquipmentLocation: device.EquipmentLocation,
                    RequiredPerformanceLevel: classification.RequiredPerformanceLevel,
                    AchievedPerformanceLevel: classification.AchievedPerformanceLevel,
                    Category: classification.Category,
                    InspectedAtUtc: device.CalibratedAtUtc,
                    NextInspectionDueAtUtc: device.NextCalibrationDueAtUtc,
                    FaultCount: device.TriggerEventCount,
                    ReliabilityOrAccuracyScore: device.DetectionAccuracyScore));
            }

            var input = new ControlSystemSafetyInput(
                Components: snapshots,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.Standards.ControlSystemSafetyEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate ISO 13849-1 / IEC 62061 compliance for the requested tenant/site.",
                    Guid.NewGuid()));
            }

            return Result<StandardsComplianceReportDto>.Success(StandardsComplianceReportMapper.ToDto(evaluation.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate ISO 13849-1 / IEC 62061 control system safety compliance for Tenant {TenantId}, Site {SiteId}", request.TenantId, request.SiteId);

            return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.Standards.ControlSystemSafetyEvaluationFailed",
                "Unable to evaluate ISO 13849-1 / IEC 62061 control system safety compliance at this time.",
                Guid.NewGuid()));
        }
    }

    /// <summary>
    /// Resolves the caller-supplied classification for a real component by title, or a
    /// conservative default (lowest required Performance Level, no achieved rating) when the
    /// caller did not supply one -- see command header.
    /// </summary>
    private static ControlSystemComponentClassificationInput ResolveClassification(
        string componentIdentifier, IReadOnlyDictionary<string, ControlSystemComponentClassificationInput> classificationsByIdentifier)
    {
        return classificationsByIdentifier.TryGetValue(componentIdentifier, out var classification)
            ? classification
            : new ControlSystemComponentClassificationInput(componentIdentifier, PerformanceLevel.PLa, null, null);
    }
}
