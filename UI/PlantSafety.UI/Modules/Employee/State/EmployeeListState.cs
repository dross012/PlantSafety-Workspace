// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Employee.State
//  File:           EmployeeListState.cs
//  Created:        2026-08-09
//  Description:    State container for the Employee list page (Section 9).
// ============================================================================
using PlantSafety.UI.Modules.Employee.Mappings;
using PlantSafety.UI.Modules.Employee.Services;

namespace PlantSafety.UI.Modules.Employee.State;

/// <summary>State container for the Employee list page.</summary>
public sealed class EmployeeListState(EmployeeUiService employeeUiService) : IAsyncDisposable
{
    /// <summary>True while the list is loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last load failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last load's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current employees, or empty before the first load.</summary>
    public IReadOnlyList<EmployeeViewModel> Employees { get; private set; } = [];

    /// <summary>Loads every employee.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Employees = await employeeUiService.GetAllAsync();
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

    /// <summary>Reloads every employee.</summary>
    public Task RefreshAsync() => LoadAsync();

    /// <summary>Resets the list to empty.</summary>
    public void Reset()
    {
        Employees = [];
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
