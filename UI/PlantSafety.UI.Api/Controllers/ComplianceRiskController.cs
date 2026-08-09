// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           ComplianceRiskController.cs
//  Created:        2026-08-09
//  Description:    Exposes Shared.SafetyData's real IComplianceRiskService,
//                  including Phase C.10's severity-aware compliance/risk
//                  scoring -- previously built but never reachable from the
//                  UI (flagged as a known gap when PlantSafety.UI was first
//                  built; this closes it). Builds the FacilityProfile the
//                  same way FacilitiesController-adjacent flows would (via
//                  IFacilityProfileService), never hand-constructs one.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for a severity-aware compliance/risk assessment.</summary>
public sealed record AssessFacilityRequest(
    Guid FacilityId,
    string Name,
    string IndustryName,
    string Location,
    IReadOnlyList<string>? CompletedElementCodes,
    SeverityLevel EventSeverity,
    int EmployeeInjuryCount,
    int MachineFailureCount,
    int EventChainLength);

/// <summary>The combined result of a severity-aware compliance and risk assessment.</summary>
public sealed record AssessFacilityResponse(
    SeverityAwareComplianceScore ComplianceScore,
    SeverityAwareRiskScore RiskScore);

/// <summary>Exposes Shared.SafetyData's real Compliance &amp; Risk subsystem, including Phase C.10's severity-aware scoring.</summary>
[ApiController]
[Route("api/v1/ui/compliance-risk")]
public sealed class ComplianceRiskController(IFacilityProfileService facilityProfileService, IComplianceRiskService complianceRiskService) : ControllerBase
{
    /// <summary>
    /// POST api/v1/ui/compliance-risk/assess -- builds a FacilityProfile for the given industry/
    /// completion state, then returns both its severity-aware compliance score and severity-aware
    /// risk score for the given triggering event context.
    /// </summary>
    /// <exception cref="ArgumentException">No industry named <see cref="AssessFacilityRequest.IndustryName"/> exists.</exception>
    [HttpPost("assess")]
    public ActionResult<AssessFacilityResponse> Assess([FromBody] AssessFacilityRequest request)
    {
        try
        {
            FacilityProfile facility = facilityProfileService.BuildProfile(
                request.FacilityId,
                request.Name,
                request.IndustryName,
                request.Location,
                request.CompletedElementCodes?.ToHashSet(StringComparer.Ordinal));

            SeverityAwareComplianceScore complianceScore = complianceRiskService.ComputeSeverityAwareComplianceScore(facility, request.EventSeverity);
            SeverityAwareRiskScore riskScore = complianceRiskService.ComputeSeverityAwareRiskScore(
                facility,
                request.EventSeverity,
                request.EmployeeInjuryCount,
                request.MachineFailureCount,
                request.EventChainLength);

            return Ok(new AssessFacilityResponse(complianceScore, riskScore));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
