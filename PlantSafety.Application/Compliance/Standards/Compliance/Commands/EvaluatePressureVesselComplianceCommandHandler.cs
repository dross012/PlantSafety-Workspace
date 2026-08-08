// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.PressureVessel
//  Layer:          Application
//  File:           EvaluatePressureVesselComplianceCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluatePressureVesselComplianceCommand. Maps the command's
//                  caller-supplied vessel records directly into PressureVesselComplianceInput (no
//                  repository involved -- see command header for why) and invokes
//                  PressureVesselComplianceEngine.EvaluateCompliance(...), translating the
//                  Domain-layer Result&lt;StandardsComplianceReport&gt; into the Application-layer
//                  Result&lt;StandardsComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Standards.PressureVessel.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Handles <see cref="EvaluatePressureVesselComplianceCommand"/>.</summary>
public sealed class EvaluatePressureVesselComplianceCommandHandler : IRequestHandler<EvaluatePressureVesselComplianceCommand, Result<StandardsComplianceReportDto>>
{
    private static readonly PressureVesselComplianceEngine Engine = new();

    private readonly ILogger<EvaluatePressureVesselComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluatePressureVesselComplianceCommandHandler(ILogger<EvaluatePressureVesselComplianceCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<StandardsComplianceReportDto>> Handle(EvaluatePressureVesselComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var vessels = request.Vessels.Select(v => new PressureVesselRecord(
                VesselIdentifier: v.VesselIdentifier,
                VesselType: v.VesselType,
                MaxAllowableWorkingPressurePsi: v.MaxAllowableWorkingPressurePsi,
                LastInspectionAtUtc: v.LastInspectionAtUtc.HasValue ? UtcTimestamp.From(v.LastInspectionAtUtc.Value) : null,
                NextInspectionDueAtUtc: v.NextInspectionDueAtUtc.HasValue ? UtcTimestamp.From(v.NextInspectionDueAtUtc.Value) : null,
                HasCurrentJurisdictionalCertificate: v.HasCurrentJurisdictionalCertificate,
                NewOrReplacementInstallationPending: v.NewOrReplacementInstallationPending,
                InstallationNoticeFiledAtUtc: v.InstallationNoticeFiledAtUtc.HasValue ? UtcTimestamp.From(v.InstallationNoticeFiledAtUtc.Value) : null)).ToList();

            var input = new PressureVesselComplianceInput(
                Vessels: vessels,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Task.FromResult(Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.Standards.PressureVesselEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate ASME BPVC compliance for the supplied pressure-vessel inventory.",
                    Guid.NewGuid())));
            }

            return Task.FromResult(Result<StandardsComplianceReportDto>.Success(StandardsComplianceReportMapper.ToDto(evaluation.Value!)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate ASME BPVC pressure-vessel compliance for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.Standards.PressureVesselEvaluationFailed",
                "Unable to evaluate ASME BPVC pressure-vessel compliance at this time.",
                Guid.NewGuid())));
        }
    }
}
