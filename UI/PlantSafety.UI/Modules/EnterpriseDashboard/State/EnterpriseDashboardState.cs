// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.EnterpriseDashboard.State
//  File:           EnterpriseDashboardState.cs
//  Created:        2026-08-09
//  Description:    State container for the Enterprise Dashboard page.
// ============================================================================
using PlantSafety.UI.Modules.EnterpriseDashboard.Services;

namespace PlantSafety.UI.Modules.EnterpriseDashboard.State;

/// <summary>State container for the Enterprise Dashboard page.</summary>
public sealed class EnterpriseDashboardState(EnterpriseDashboardUiService uiService) : IAsyncDisposable
{
    /// <summary>True while loading.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last load failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last load's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The formatted dashboard payload JSON, or null before load.</summary>
    public string? DashboardJson { get; private set; }

    /// <summary>Loads the aggregated enterprise dashboard.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            DashboardJson = await uiService.GetDashboardJsonAsync();
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

    /// <summary>Reloads the dashboard.</summary>
    public Task RefreshAsync() => LoadAsync();

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
