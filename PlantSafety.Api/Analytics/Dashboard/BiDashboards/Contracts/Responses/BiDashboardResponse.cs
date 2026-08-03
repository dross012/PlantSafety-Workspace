// ============================================================================
// File: BiDashboardResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: Generic external-facing response DTO returned inside ApiResponse<T> for every
//              BI Dashboards KPI section endpoint.
// ============================================================================
//
// Onboarding Notes:
// - Mirrors PlantSafety.Api.Analytics.Dashboard.Executive.Contracts.Responses.ExecutiveResponse,
//   generalised over the item type since each of the six BI Dashboard sub-features has its own
//   item DTO shape. Matches PlantSafety.UI.Modules.BiDashboards.ApiClients.BiDashboardsApiClient's
//   BiDashboardSectionDto&lt;TItem&gt; wire shape field-for-field.
//
// Rationale:
// - Keeps the external contract stable even when Application-layer result shapes change.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Dashboard.BiDashboards.Contracts.Responses;

/// <summary>Response payload for a single BI Dashboards KPI section query.</summary>
/// <typeparam name="TItem">The item type contained in <see cref="Items"/>.</typeparam>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the BI Dashboard read-model. Never null; empty when no results.</param>
public sealed record BiDashboardResponse<TItem>(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<TItem>  Items);
