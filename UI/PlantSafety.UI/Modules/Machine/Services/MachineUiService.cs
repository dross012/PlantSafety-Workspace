// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Machine.Services
//  File:           MachineUiService.cs
//  Created:        2026-08-09
//  Description:    The only layer allowed to call PlantSafety.UI.Api for the
//                  Machine module (Section 10).
// ============================================================================
using PlantSafety.UI.Modules.Machine.Mappings;
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.Machine.Services;

/// <summary>The request body for creating/updating a machine's core metadata.</summary>
public sealed record MachineMetadataRequest(string Name, string MachineType, Guid FacilityId, string AreaDescription);

/// <summary>Calls PlantSafety.UI.Api's Machines endpoints and maps results to UI ViewModels.</summary>
public sealed class MachineUiService(PlantSafetyApiClient apiClient)
{
    private const string BaseRoute = "api/v1/ui/machines";

    /// <summary>Fetches every machine as UI ViewModels.</summary>
    public async Task<IReadOnlyList<MachineViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MachineBackendDto>? dtos = await apiClient.GetAsync<IReadOnlyList<MachineBackendDto>>(BaseRoute, cancellationToken);
        return dtos?.Select(d => d.ToViewModel()).ToArray() ?? [];
    }

    /// <summary>Fetches a single machine as a UI ViewModel, or null if not found.</summary>
    public async Task<MachineViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        MachineBackendDto? dto = await apiClient.GetAsync<MachineBackendDto>($"{BaseRoute}/{id}", cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Creates a new machine and returns its UI ViewModel.</summary>
    public async Task<MachineViewModel?> CreateAsync(MachineMetadataRequest request, CancellationToken cancellationToken = default)
    {
        MachineBackendDto? dto = await apiClient.PostAsync<MachineMetadataRequest, MachineBackendDto>(BaseRoute, request, cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Records a failure for the given machine and returns its updated UI ViewModel.</summary>
    public async Task<MachineViewModel?> RecordFailureAsync(Guid id, string description, string resultingHazardType, CancellationToken cancellationToken = default)
    {
        MachineBackendDto? dto = await apiClient.PostAsync<object, MachineBackendDto>(
            $"{BaseRoute}/{id}/failures", new { Description = description, ResultingHazardType = resultingHazardType }, cancellationToken);
        return dto?.ToViewModel();
    }
}
