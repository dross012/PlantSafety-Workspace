// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Machine.State
//  File:           MachineListState.cs
//  Created:        2026-08-09
//  Description:    State container for the Machine list page (Section 9).
// ============================================================================
using PlantSafety.UI.Modules.Machine.Mappings;
using PlantSafety.UI.Modules.Machine.Services;

namespace PlantSafety.UI.Modules.Machine.State;

/// <summary>State container for the Machine list page.</summary>
public sealed class MachineListState(MachineUiService machineUiService) : IAsyncDisposable
{
    /// <summary>True while the list is loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last load failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last load's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current machines, or empty before the first load.</summary>
    public IReadOnlyList<MachineViewModel> Machines { get; private set; } = [];

    /// <summary>Loads every machine.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Machines = await machineUiService.GetAllAsync();
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

    /// <summary>Reloads every machine.</summary>
    public Task RefreshAsync() => LoadAsync();

    /// <summary>Resets the list to empty.</summary>
    public void Reset()
    {
        Machines = [];
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
