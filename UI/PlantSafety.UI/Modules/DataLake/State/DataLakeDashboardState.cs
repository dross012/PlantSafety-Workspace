// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.DataLake.State
//  File:           DataLakeDashboardState.cs
//  Created:        2026-08-09
//  Description:    State container for the Data Lake dashboard page.
// ============================================================================
using PlantSafety.UI.Modules.DataLake.Services;

namespace PlantSafety.UI.Modules.DataLake.State;

/// <summary>State container for the Data Lake dashboard page.</summary>
public sealed class DataLakeDashboardState(DataLakeUiService uiService) : IAsyncDisposable
{
    /// <summary>True while loading/writing.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last operation failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last operation's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current per-record-type counts.</summary>
    public IReadOnlyDictionary<string, int> Counts { get; private set; } = new Dictionary<string, int>();

    /// <summary>Loads the count summary.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Counts = await uiService.GetCountsAsync() ?? new Dictionary<string, int>();
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

    /// <summary>Reloads the count summary.</summary>
    public Task RefreshAsync() => LoadAsync();

    /// <summary>Writes a new record and reloads the count summary.</summary>
    public async Task WriteAsync(string recordType, Guid sourceId, string payload)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            await uiService.WriteAsync(recordType, sourceId, payload);
            await LoadAsync();
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

    /// <summary>Resets to empty.</summary>
    public void Reset()
    {
        Counts = new Dictionary<string, int>();
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
