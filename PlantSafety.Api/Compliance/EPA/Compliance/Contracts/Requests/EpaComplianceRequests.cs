// ============================================================================
// File: EpaComplianceRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.EPA.Compliance
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body for the Compliance.EPA.Compliance
//              V1 controller's five EPA compliance-evaluation endpoints (RCRA, TSCA, CERCLA/SARA,
//              Clean Air Act, Clean Water Act). Named with the "Epa" spelling throughout (never
//              bare "EPA") to avoid collision with the pre-existing, unrelated bare-"EPA"
//              identifiers used by Compliance.Certification.EPA and
//              Compliance.RegulatoryCalendar.Deadlines.EPA. Mirrors
//              PlantSafety.Api.Compliance.PHMSA.Classification.Contracts.Requests
//              .HazmatClassificationRequests.cs.
// ============================================================================

using PlantSafety.Application.Compliance.EPA.Compliance.Commands;

namespace PlantSafety.Api.Compliance.EPA.Compliance.Contracts.Requests;

/// <summary>Request body for evaluating RCRA hazardous-waste-management compliance for a site.</summary>
public sealed record EpaRcraComplianceRequestDto
{
    /// <summary>Site whose RCRA posture is being evaluated.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Identifiers of the site's accumulation areas whose containers are in scope for this evaluation.</summary>
    public required IReadOnlyList<Guid> AccumulationAreaIds { get; init; }

    /// <summary>Total hazardous waste generated in the evaluation calendar month, in kilograms.</summary>
    public required decimal MonthlyHazardousWasteQuantityKg { get; init; }

    /// <summary>Portion of the monthly quantity that is acute hazardous waste (P-listed), in kilograms.</summary>
    public required decimal MonthlyAcuteHazardousWasteQuantityKg { get; init; }
}

/// <summary>Request body for evaluating TSCA chemical-substance compliance for a caller-supplied chemical inventory.</summary>
public sealed record EpaTscaComplianceRequestDto
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The chemical inventory snapshot to evaluate.</summary>
    public required IReadOnlyList<TscaChemicalRecordInput> Chemicals { get; init; }
}

/// <summary>Request body for evaluating CERCLA Section 103 / EPCRA Section 304 release-reporting obligations for a release event.</summary>
public sealed record EpaCerclaReportingRequestDto
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional traceability link to the real SafetyEvents.IncidentManagement aggregate this release corresponds to, if any.</summary>
    public Guid? LinkedIncidentManagementId { get; init; }

    /// <summary>Name/identity of the released substance.</summary>
    public required string SubstanceName { get; init; }

    /// <summary>CAS registry number of the substance, if known.</summary>
    public string? CasNumber { get; init; }

    /// <summary>Quantity released, in <see cref="QuantityUnit"/>.</summary>
    public required decimal ReleasedQuantity { get; init; }

    /// <summary>Unit of measure for the released/threshold quantities (e.g. "lbs", "kg").</summary>
    public required string QuantityUnit { get; init; }

    /// <summary>The substance's own CERCLA reportable quantity (RQ) per 40 CFR 302 Table 302.4.</summary>
    public required decimal ReportableQuantityThreshold { get; init; }

    /// <summary>EPCRA Section 302 Extremely Hazardous Substance (EHS) list membership.</summary>
    public required bool IsExtremelyHazardousSubstance { get; init; }

    /// <summary>UTC instant the release occurred.</summary>
    public required DateTime ReleaseOccurredAtUtc { get; init; }
}

/// <summary>Request body for evaluating Clean Air Act compliance for a caller-supplied set of air emission sources.</summary>
public sealed record EpaCleanAirComplianceRequestDto
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The emission-source snapshot to evaluate.</summary>
    public required IReadOnlyList<EmissionSourceRecordInput> EmissionSources { get; init; }
}

/// <summary>Request body for evaluating Clean Water Act compliance for a site's discharge permits, limit exceedances, and SPCC applicability.</summary>
public sealed record EpaCleanWaterComplianceRequestDto
{
    /// <summary>Site whose Clean Water Act posture is being evaluated.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Identifiers of the site's discharge permits in scope for this evaluation.</summary>
    public required IReadOnlyList<Guid> DischargePermitIds { get; init; }

    /// <summary>Whether the facility stores oil above the SPCC applicability threshold (40 CFR Part 112).</summary>
    public required bool FacilityStoresOilAboveSpccThreshold { get; init; }

    /// <summary>Whether the facility has a Spill Prevention, Control, and Countermeasure plan.</summary>
    public required bool HasSpccPlan { get; init; }

    /// <summary>UTC instant the SPCC plan was last reviewed/amended, if any.</summary>
    public DateTime? SpccPlanLastReviewedAtUtc { get; init; }
}
