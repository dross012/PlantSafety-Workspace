// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Facility.State
//  File:           FacilityDetailState.cs
//  Created:        2026-08-09
//  Description:    State container for the Facility detail page (Section 9).
// ============================================================================
using PlantSafety.UI.Modules.Facility.Mappings;
using PlantSafety.UI.Modules.Facility.Services;

namespace PlantSafety.UI.Modules.Facility.State;

/// <summary>State container for the Facility detail page.</summary>
public sealed class FacilityDetailState(FacilityUiService facilityUiService) : IAsyncDisposable
{
    /// <summary>True while loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last operation failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last operation's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current facility, or null before load / if not found.</summary>
    public FacilityViewModel? Facility { get; private set; }

    /// <summary>Loads the facility with the given id.</summary>
    public async Task LoadAsync(Guid facilityId)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Facility = await facilityUiService.GetByIdAsync(facilityId);
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

    /// <summary>Activates the current facility.</summary>
    public Task ActivateAsync() => RunActionAsync(id => facilityUiService.ActivateAsync(id));

    /// <summary>Shuts down the current facility.</summary>
    public Task ShutdownAsync(string reason) => RunActionAsync(id => facilityUiService.ShutdownAsync(id, reason));

    /// <summary>Reopens the current facility.</summary>
    public Task ReopenAsync() => RunActionAsync(id => facilityUiService.ReopenAsync(id));

    /// <summary>Decommissions the current facility.</summary>
    public Task DecommissionAsync(string reason) => RunActionAsync(id => facilityUiService.DecommissionAsync(id, reason));

    /// <summary>Resets the detail state.</summary>
    public void Reset()
    {
        Facility = null;
        HasError = false;
        Error = null;
    }

    private async Task RunActionAsync(Func<Guid, Task<FacilityViewModel?>> action)
    {
        if (Facility is null)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Facility = await action(Facility.FacilityId);
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

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
