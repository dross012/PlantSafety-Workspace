// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Facility.Services
//  File:           FacilityUiService.cs
//  Created:        2026-08-09
//  Description:    The only layer allowed to call PlantSafety.UI.Api for the
//                  Facility module (Section 10). Calls the backend via
//                  PlantSafetyApiClient, maps backend DTOs -&gt; UI
//                  ViewModels, never contains domain/business/rendering/
//                  navigation logic.
// ============================================================================
using PlantSafety.UI.Modules.Facility.Mappings;
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.Facility.Services;

/// <summary>The request body for creating/updating a facility's core metadata.</summary>
public sealed record FacilityMetadataRequest(string Name, string IndustryName, string? NaicsCode, FacilityLocationRequest Location);

/// <summary>The request body's location shape.</summary>
public sealed record FacilityLocationRequest(string AddressLine1, string City, string StateOrProvince, string PostalCode, string CountryCode);

/// <summary>Calls PlantSafety.UI.Api's Facilities endpoints and maps results to UI ViewModels.</summary>
public sealed class FacilityUiService(PlantSafetyApiClient apiClient)
{
    private const string BaseRoute = "api/v1/ui/facilities";

    /// <summary>Fetches every facility as UI ViewModels.</summary>
    public async Task<IReadOnlyList<FacilityViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FacilityBackendDto>? dtos = await apiClient.GetAsync<IReadOnlyList<FacilityBackendDto>>(BaseRoute, cancellationToken);
        return dtos?.Select(d => d.ToViewModel()).ToArray() ?? [];
    }

    /// <summary>Fetches a single facility as a UI ViewModel, or null if not found.</summary>
    public async Task<FacilityViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        FacilityBackendDto? dto = await apiClient.GetAsync<FacilityBackendDto>($"{BaseRoute}/{id}", cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Creates a new facility and returns its UI ViewModel.</summary>
    public async Task<FacilityViewModel?> CreateAsync(FacilityMetadataRequest request, CancellationToken cancellationToken = default)
    {
        FacilityBackendDto? dto = await apiClient.PostAsync<FacilityMetadataRequest, FacilityBackendDto>(BaseRoute, request, cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Activates a facility and returns its updated UI ViewModel.</summary>
    public async Task<FacilityViewModel?> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        FacilityBackendDto? dto = await apiClient.PostAsync<object?, FacilityBackendDto>($"{BaseRoute}/{id}/activate", null, cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Shuts down a facility and returns its updated UI ViewModel.</summary>
    public async Task<FacilityViewModel?> ShutdownAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        FacilityBackendDto? dto = await apiClient.PostAsync<object, FacilityBackendDto>($"{BaseRoute}/{id}/shutdown", new { Reason = reason }, cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Reopens a facility and returns its updated UI ViewModel.</summary>
    public async Task<FacilityViewModel?> ReopenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        FacilityBackendDto? dto = await apiClient.PostAsync<object?, FacilityBackendDto>($"{BaseRoute}/{id}/reopen", null, cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Decommissions a facility and returns its updated UI ViewModel.</summary>
    public async Task<FacilityViewModel?> DecommissionAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        FacilityBackendDto? dto = await apiClient.PostAsync<object, FacilityBackendDto>($"{BaseRoute}/{id}/decommission", new { Reason = reason }, cancellationToken);
        return dto?.ToViewModel();
    }
}
