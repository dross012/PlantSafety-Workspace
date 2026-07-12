// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEmergencyManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Queries;

/// <summary>Result of <see cref="GetEmergencyManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EmergencyManagement items.</param>
public sealed record EmergencyManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencyManagementItemDto> Items);
