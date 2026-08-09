// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.TenantFacilityRegistry.Services
//  File:           TenantFacilityRegistryUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's TenantFacilityRegistryController
//                  (Shared.SafetyData's real multi-facility tenant registry,
//                  Phase C.8).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.TenantFacilityRegistry.Services;

/// <summary>Local mirror of TenantFacilityMap.</summary>
public sealed record TenantFacilityMapDto(string TenantId, IReadOnlyList<Guid> FacilityIds, Guid? PrimaryFacilityId);

/// <summary>The request body for registering a facility to a tenant.</summary>
public sealed record RegisterFacilityRequest(string TenantId, Guid FacilityId, bool IsPrimary);

/// <summary>Calls PlantSafety.UI.Api's Tenant Facility Registry endpoints.</summary>
public sealed class TenantFacilityRegistryUiService(PlantSafetyApiClient apiClient)
{
    private const string BaseRoute = "api/v1/ui/tenant-facility-registry";

    /// <summary>Fetches every tenant/facility binding.</summary>
    public async Task<IReadOnlyList<TenantFacilityMapDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await apiClient.GetAsync<IReadOnlyList<TenantFacilityMapDto>>(BaseRoute, cancellationToken) ?? [];

    /// <summary>Registers a facility to a tenant.</summary>
    public Task<string> RegisterAsync(string tenantId, Guid facilityId, bool isPrimary, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync(BaseRoute, System.Text.Json.JsonSerializer.Serialize(new RegisterFacilityRequest(tenantId, facilityId, isPrimary)), cancellationToken);
}
