// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.CleanWaterAct
//  Layer:          Application
//  File:           EvaluateCleanWaterComplianceCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate Clean Water Act (33 U.S.C. Section1251, 40 CFR Parts
//                  122/112) compliance for a site by loading its real DischargePermit and
//                  LimitExceedance read-model data and running it through the Domain-layer
//                  CleanWaterComplianceEngine. Carries the full command envelope
//                  (CorrelationId/CausationId/CommandId/RequestedAtUtc/ApiVersionToken) per
//                  Handbook Rule 2.1.
//
//                  ASSUMPTION (documented per Handbook ambiguity-resolution rule): the real
//                  Application-layer DischargePermit read repository
//                  (PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance
//                  .DischargePermit.Repositories.IDischargePermitRepository) only exposes
//                  GetDischargePermitByIdAsync(tenantId, dischargePermitId) -- there is no
//                  existing "list all permits for a site" read method anywhere in this codebase.
//                  Rather than fabricate a nonexistent repository call, this command accepts the
//                  explicit set of DischargePermitIds to evaluate, and the handler calls the real,
//                  existing by-id repository method for each. LimitExceedance data, by contrast,
//                  DOES have a real site-scoped list method (ListActiveExceedancesAsync) and is
//                  loaded directly by SiteId. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Evaluates Clean Water Act compliance for a site's discharge permits, limit exceedances, and SPCC applicability.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Site whose Clean Water Act posture is being evaluated.</param>
/// <param name="DischargePermitIds">
/// Identifiers of the site's discharge permits in scope for this evaluation (see ASSUMPTION in the
/// file header -- no site-wide permit listing exists yet).
/// </param>
/// <param name="FacilityStoresOilAboveSpccThreshold">Whether the facility stores oil above the SPCC applicability threshold (40 CFR Part 112).</param>
/// <param name="HasSpccPlan">Whether the facility has a Spill Prevention, Control, and Countermeasure plan.</param>
/// <param name="SpccPlanLastReviewedAtUtc">UTC instant the SPCC plan was last reviewed/amended, if any.</param>
public sealed record EvaluateCleanWaterComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid SiteId,
    IReadOnlyList<Guid> DischargePermitIds,
    bool FacilityStoresOilAboveSpccThreshold,
    bool HasSpccPlan,
    DateTime? SpccPlanLastReviewedAtUtc) : ICommand<Result<EpaComplianceReportDto>>;
