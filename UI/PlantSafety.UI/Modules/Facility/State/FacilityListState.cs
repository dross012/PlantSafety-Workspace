// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Facility.State
//  File:           FacilityListState.cs
//  Created:        2026-08-09
//  Description:    State container for the Facility list page (Section 9).
//                  Deterministic, no domain/backend-DTO exposure -- only
//                  FacilityViewModel (UI DTOs).
// ============================================================================
using PlantSafety.UI.Modules.Facility.Mappings;
using PlantSafety.UI.Modules.Facility.Services;

namespace PlantSafety.UI.Modules.Facility.State;

/// <summary>State container for the Facility list page.</summary>
public sealed class FacilityListState(FacilityUiService facilityUiService) : IAsyncDisposable
{
    /// <summary>True while the list is loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last load failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last load's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current facilities, or empty before the first load.</summary>
    public IReadOnlyList<FacilityViewModel> Facilities { get; private set; } = [];

    /// <summary>Loads every facility.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Facilities = await facilityUiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Reloads every facility.</summary>
    public Task RefreshAsync() => LoadAsync();

    /// <summary>Resets the list to empty.</summary>
    public void Reset()
    {
        Facilities = [];
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
