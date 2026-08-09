// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Machine.State
//  File:           MachineDetailState.cs
//  Created:        2026-08-09
//  Description:    State container for the Machine detail page (Section 9).
// ============================================================================
using PlantSafety.UI.Modules.Machine.Mappings;
using PlantSafety.UI.Modules.Machine.Services;

namespace PlantSafety.UI.Modules.Machine.State;

/// <summary>State container for the Machine detail page.</summary>
public sealed class MachineDetailState(MachineUiService machineUiService) : IAsyncDisposable
{
    /// <summary>True while loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last operation failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last operation's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current machine, or null before load / if not found.</summary>
    public MachineViewModel? Machine { get; private set; }

    /// <summary>Loads the machine with the given id.</summary>
    public async Task LoadAsync(Guid machineId)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Machine = await machineUiService.GetByIdAsync(machineId);
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

    /// <summary>Records a failure for the current machine.</summary>
    public async Task RecordFailureAsync(string description, string resultingHazardType)
    {
        if (Machine is null)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Machine = await machineUiService.RecordFailureAsync(Machine.MachineId, description, resultingHazardType);
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

    /// <summary>Resets the detail state.</summary>
    public void Reset()
    {
        Machine = null;
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
