// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Application
//  File:           CostCenterMapping.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Simple reference-data record mapping a Tenant/Site to its external ERP cost
//                  center code. Used by financial-posting and inventory outbound hooks to enrich
//                  outbound envelopes with the ERP's own cost-center identifier. Deliberately a flat
//                  reference-data record (not a rich aggregate) -- this is lookup data with no
//                  lifecycle or invariants beyond uniqueness per Tenant/Site, matching the Handbook's
//                  ValueObject Appropriateness Rule guidance that not every concept needs aggregate
//                  ceremony. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;

/// <summary>Maps a Tenant/Site pair to its external ERP cost center code.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="SiteId">Site identifier, or null for a tenant-wide default mapping.</param>
/// <param name="ExternalCostCenterCode">The external ERP system's cost center code (e.g. SAP Kostenstelle).</param>
/// <param name="Description">Human-readable description of the cost center.</param>
/// <param name="UpdatedAtUtc">UTC instant this mapping was last set.</param>
/// <param name="UpdatedBy">Identity of the user who last set this mapping.</param>
public sealed record CostCenterMapping(
    Guid TenantId,
    Guid? SiteId,
    string ExternalCostCenterCode,
    string? Description,
    UtcTimestamp UpdatedAtUtc,
    UserId UpdatedBy);
