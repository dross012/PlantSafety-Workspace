// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Employee.State
//  File:           EmployeeDetailState.cs
//  Created:        2026-08-09
//  Description:    State container for the Employee detail page (Section 9).
// ============================================================================
using PlantSafety.UI.Modules.Employee.Mappings;
using PlantSafety.UI.Modules.Employee.Services;

namespace PlantSafety.UI.Modules.Employee.State;

/// <summary>State container for the Employee detail page.</summary>
public sealed class EmployeeDetailState(EmployeeUiService employeeUiService) : IAsyncDisposable
{
    /// <summary>True while loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last operation failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last operation's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current employee, or null before load / if not found.</summary>
    public EmployeeViewModel? Employee { get; private set; }

    /// <summary>Loads the employee with the given id.</summary>
    public async Task LoadAsync(Guid employeeId)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Employee = await employeeUiService.GetByIdAsync(employeeId);
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

    /// <summary>Records an injury for the current employee.</summary>
    public async Task RecordInjuryAsync(string description)
    {
        if (Employee is null)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Employee = await employeeUiService.RecordInjuryAsync(Employee.EmployeeId, description);
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
        Employee = null;
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
