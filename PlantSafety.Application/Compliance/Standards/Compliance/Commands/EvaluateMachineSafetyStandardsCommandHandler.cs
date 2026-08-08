// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.MachineSafety
//  Layer:          Application
//  File:           EvaluateMachineSafetyStandardsCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateMachineSafetyStandardsCommand. Loads real
//                  MachineGuardsBarriers, LotoSystems, EmergencyStopDevices, and TwoHandControls
//                  aggregate data for the tenant/site via their real Domain repository interfaces,
//                  projects each into the Domain engine's MachineGuardSnapshot/LotoSystemSnapshot/
//                  EmergencyStopSnapshot/TwoHandControlSnapshot records, invokes
//                  MachineSafetyStandardsEngine.EvaluateCompliance(...), and translates the
//                  Domain-layer Result&lt;StandardsComplianceReport&gt; into the Application-layer
//                  Result&lt;StandardsComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Standards.MachineSafety.Services;
using PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Interfaces;
using PlantSafety.Domain.MachineSafety.LotoSystems.Interfaces;
using PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Interfaces;
using PlantSafety.Domain.MachineSafety.TwoHandControls.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;
using PlantSafety.Application.Compliance.Standards.Compliance.Specifications;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateMachineSafetyStandardsCommand"/>.</summary>
public sealed class EvaluateMachineSafetyStandardsCommandHandler : IRequestHandler<EvaluateMachineSafetyStandardsCommand, Result<StandardsComplianceReportDto>>
{
    private static readonly MachineSafetyStandardsEngine Engine = new();

    private readonly IMachineGuardsBarriersRepository _machineGuardsBarriersRepository;
    private readonly ILotoSystemsRepository _lotoSystemsRepository;
    private readonly IEmergencyStopDevicesRepository _emergencyStopDevicesRepository;
    private readonly ITwoHandControlsRepository _twoHandControlsRepository;
    private readonly ILogger<EvaluateMachineSafetyStandardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateMachineSafetyStandardsCommandHandler(
        IMachineGuardsBarriersRepository machineGuardsBarriersRepository,
        ILotoSystemsRepository lotoSystemsRepository,
        IEmergencyStopDevicesRepository emergencyStopDevicesRepository,
        ITwoHandControlsRepository twoHandControlsRepository,
        ILogger<EvaluateMachineSafetyStandardsCommandHandler> logger)
    {
        _machineGuardsBarriersRepository = machineGuardsBarriersRepository;
        _lotoSystemsRepository = lotoSystemsRepository;
        _emergencyStopDevicesRepository = emergencyStopDevicesRepository;
        _twoHandControlsRepository = twoHandControlsRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StandardsComplianceReportDto>> Handle(EvaluateMachineSafetyStandardsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var guardSpecification = new MachineGuardsBarriersTenantSiteSpecification(request.TenantId, request.SiteId);
            var guards = await _machineGuardsBarriersRepository.ListAsync(guardSpecification, cancellationToken);

            var lotoSpecification = new LotoSystemsTenantSiteSpecification(request.TenantId, request.SiteId);
            var lotoSystems = await _lotoSystemsRepository.ListAsync(lotoSpecification, cancellationToken);

            var eStopSpecification = new EmergencyStopDevicesTenantSiteSpecification(request.TenantId, request.SiteId);
            var eStops = await _emergencyStopDevicesRepository.ListAsync(eStopSpecification, cancellationToken);

            var twoHandSpecification = new TwoHandControlsTenantSiteSpecification(request.TenantId, request.SiteId);
            var twoHandControls = await _twoHandControlsRepository.ListAsync(twoHandSpecification, cancellationToken);

            var guardSnapshots = guards.Select(g => new MachineGuardSnapshot(
                GuardTitle: g.GuardTitle,
                GuardType: g.GuardType.ToString(),
                EquipmentLocation: g.EquipmentLocation,
                InspectedAtUtc: g.InspectedAtUtc,
                NextInspectionDueAtUtc: g.NextInspectionDueAtUtc,
                DeficiencyCount: g.DeficiencyCount,
                GuardIntegrityScore: g.GuardIntegrityScore ?? 0m,
                Status: g.Status.ToString())).ToList();

            var lotoSnapshots = lotoSystems.Select(l => new LotoSystemSnapshot(
                SystemTitle: l.SystemTitle,
                EquipmentTag: l.EquipmentTag,
                EnergySourceTypes: l.EnergySourceTypes.ToString(),
                IsolationPointCount: l.IsolationPointCount,
                VerifiedZeroEnergyAtUtc: l.VerifiedZeroEnergyAtUtc,
                AuditedAtUtc: l.AuditedAtUtc,
                ComplianceScore: l.ComplianceScore ?? 0m,
                Status: l.Status.ToString())).ToList();

            var eStopSnapshots = eStops.Select(s => new EmergencyStopSnapshot(
                DeviceTitle: s.DeviceTitle,
                DeviceType: s.DeviceType.ToString(),
                EquipmentLocation: s.EquipmentLocation,
                TestedAtUtc: s.TestedAtUtc,
                NextTestDueAtUtc: s.NextTestDueAtUtc,
                FailureCount: s.FailureCount,
                Status: s.Status.ToString())).ToList();

            var twoHandSnapshots = twoHandControls.Select(c => new TwoHandControlSnapshot(
                ControlTitle: c.ControlTitle,
                EquipmentType: c.EquipmentType.ToString(),
                EquipmentLocation: c.EquipmentLocation,
                TestedAtUtc: c.TestedAtUtc,
                NextTestDueAtUtc: c.NextTestDueAtUtc,
                BypassAttemptCount: c.BypassAttemptCount,
                Status: c.Status.ToString())).ToList();

            var input = new MachineSafetyStandardsInput(
                Guards: guardSnapshots,
                LotoSystems: lotoSnapshots,
                EmergencyStops: eStopSnapshots,
                TwoHandControls: twoHandSnapshots,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.Standards.MachineSafetyEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate machine-safety-standards compliance for the requested tenant/site.",
                    Guid.NewGuid()));
            }

            return Result<StandardsComplianceReportDto>.Success(StandardsComplianceReportMapper.ToDto(evaluation.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate machine-safety-standards compliance for Tenant {TenantId}, Site {SiteId}", request.TenantId, request.SiteId);

            return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.Standards.MachineSafetyEvaluationFailed",
                "Unable to evaluate machine-safety-standards compliance at this time.",
                Guid.NewGuid()));
        }
    }
}
