// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Employee.Services
//  File:           EmployeeUiService.cs
//  Created:        2026-08-09
//  Description:    The only layer allowed to call PlantSafety.UI.Api for the
//                  Employee module (Section 10).
// ============================================================================
using PlantSafety.UI.Modules.Employee.Mappings;
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.Employee.Services;

/// <summary>The request body for creating/updating an employee's core metadata.</summary>
public sealed record EmployeeMetadataRequest(string Name, string Role);

/// <summary>Calls PlantSafety.UI.Api's Employees endpoints and maps results to UI ViewModels.</summary>
public sealed class EmployeeUiService(PlantSafetyApiClient apiClient)
{
    private const string BaseRoute = "api/v1/ui/employees";

    /// <summary>Fetches every employee as UI ViewModels.</summary>
    public async Task<IReadOnlyList<EmployeeViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmployeeBackendDto>? dtos = await apiClient.GetAsync<IReadOnlyList<EmployeeBackendDto>>(BaseRoute, cancellationToken);
        return dtos?.Select(d => d.ToViewModel()).ToArray() ?? [];
    }

    /// <summary>Fetches a single employee as a UI ViewModel, or null if not found.</summary>
    public async Task<EmployeeViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmployeeBackendDto? dto = await apiClient.GetAsync<EmployeeBackendDto>($"{BaseRoute}/{id}", cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Creates a new employee and returns its UI ViewModel.</summary>
    public async Task<EmployeeViewModel?> CreateAsync(EmployeeMetadataRequest request, CancellationToken cancellationToken = default)
    {
        EmployeeBackendDto? dto = await apiClient.PostAsync<EmployeeMetadataRequest, EmployeeBackendDto>(BaseRoute, request, cancellationToken);
        return dto?.ToViewModel();
    }

    /// <summary>Records an injury for the given employee and returns its updated UI ViewModel.</summary>
    public async Task<EmployeeViewModel?> RecordInjuryAsync(Guid id, string description, CancellationToken cancellationToken = default)
    {
        EmployeeBackendDto? dto = await apiClient.PostAsync<object, EmployeeBackendDto>($"{BaseRoute}/{id}/injuries", new { Description = description }, cancellationToken);
        return dto?.ToViewModel();
    }
}
