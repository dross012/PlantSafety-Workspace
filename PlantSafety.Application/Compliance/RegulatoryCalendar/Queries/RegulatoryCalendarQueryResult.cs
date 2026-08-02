// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           RegulatoryCalendarQueryResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for GetRegulatoryCalendarQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.PermitsQueryResult.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Queries;

/// <summary>Result of <see cref="GetRegulatoryCalendarQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RegulatoryCalendar items.</param>
public sealed record RegulatoryCalendarQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RegulatoryCalendarItemDto> Items);
