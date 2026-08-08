// ============================================================================
// File: StandardsComplianceRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Standards.Compliance
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body for the Compliance.Standards.Compliance
//              V1 controller's six ANSI/ASME/ISO consensus-standards compliance-evaluation
//              endpoints (crane/hoist, control-system safety, machine safety, risk assessment,
//              operational safety, pressure vessel). Mirrors
//              PlantSafety.Api.Compliance.EPA.Compliance.Contracts.Requests.EpaComplianceRequests.cs.
// ============================================================================

using PlantSafety.Application.Compliance.Standards.Compliance.Commands;

namespace PlantSafety.Api.Compliance.Standards.Compliance.Contracts.Requests;

/// <summary>Request body for evaluating ASME B30 compliance for a tenant's registered crane/hoist assets.</summary>
public sealed record StandardsCraneComplianceRequestDto
{
    /// <summary>Optional site filter. Null evaluates every site's crane/hoist assets for the tenant.</summary>
    public Guid? SiteId { get; init; }
}

/// <summary>Request body for evaluating ISO 13849-1 / IEC 62061 control-system safety compliance for a site.</summary>
public sealed record StandardsControlSystemSafetyRequestDto
{
    /// <summary>Site whose control-system safety posture is being evaluated.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Caller-supplied per-component PL/Category classifications, matched against the real components loaded for this tenant/site.</summary>
    public required IReadOnlyList<ControlSystemComponentClassificationInput> Classifications { get; init; }
}

/// <summary>Request body for evaluating ANSI B11/Z244.1/Z535/ISO 12100/14120/14119 machine-safety compliance for a site.</summary>
public sealed record StandardsMachineSafetyRequestDto
{
    /// <summary>Site whose machine-safety posture is being evaluated.</summary>
    public required Guid SiteId { get; init; }
}

/// <summary>Request body for evaluating whether a documented machinery risk assessment meets ISO 12100 / ANSI B11.0 structural requirements.</summary>
public sealed record StandardsRiskAssessmentRequestDto
{
    /// <summary>Identifier of the machine/equipment the assessment covers.</summary>
    public required string EquipmentIdentifier { get; init; }

    /// <summary>The named methodology used, e.g. "ISO 12100", "ANSI B11.0 Annex C", or "NSC".</summary>
    public required string Methodology { get; init; }

    /// <summary>The hazards systematically identified during the assessment.</summary>
    public required IReadOnlyList<string> IdentifiedHazards { get; init; }

    /// <summary>Whether the assessment recorded a severity estimate for identified hazards.</summary>
    public required bool SeverityEstimated { get; init; }

    /// <summary>Whether the assessment recorded an exposure (frequency/duration) estimate.</summary>
    public required bool ExposureEstimated { get; init; }

    /// <summary>Whether the assessment recorded a probability-of-occurrence estimate.</summary>
    public required bool ProbabilityEstimated { get; init; }

    /// <summary>The risk-reduction measures documented, each tagged with its hierarchy step.</summary>
    public required IReadOnlyList<RiskReductionMeasureInput> RiskReductionMeasures { get; init; }

    /// <summary>Whether this equipment is part of a multi-machine integrated system requiring its own system-level risk assessment.</summary>
    public required bool IsIntegratedSystem { get; init; }

    /// <summary>When the documented risk assessment was completed.</summary>
    public required DateTime AssessmentCompletedAtUtc { get; init; }
}

/// <summary>Request body for evaluating ANSI Z10/ISO 45001/A1264/Z117 operational-safety compliance for a site.</summary>
public sealed record StandardsOperationalSafetyRequestDto
{
    /// <summary>Site whose operational-safety posture is being evaluated.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Caller-supplied safety-management-system structural snapshot, or null if not yet assessed.</summary>
    public SafetyManagementSystemInput? SafetyManagementSystem { get; init; }

    /// <summary>Caller-supplied walking/working-surface hazard records.</summary>
    public required IReadOnlyList<WalkingWorkingSurfaceHazardInput> SurfaceHazards { get; init; }
}

/// <summary>Request body for evaluating ASME BPVC pressure-vessel compliance for a caller-supplied vessel inventory.</summary>
public sealed record StandardsPressureVesselComplianceRequestDto
{
    /// <summary>The site's current pressure vessels/boilers to evaluate.</summary>
    public required IReadOnlyList<PressureVesselRecordInput> Vessels { get; init; }
}
