// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         State
//  File:           EngineConsoleStateBase.cs
//  Created:        2026-08-09
//  Description:    Shared base for the per-module "run this engine" console
//                  state containers (Section 9: one state container per
//                  page, deterministic, LoadAsync/RefreshAsync/Reset/
//                  DisposeAsync). Every AI.*/Enterprise.* engine console page
//                  gets its own concrete subclass in
//                  Modules/&lt;Name&gt;/State (still one container per page --
//                  this only factors out the identical Loading/Error/Ready
//                  transition plumbing every one of them needs).
// ============================================================================
namespace PlantSafety.UI.State;

/// <summary>Shared base for a "run this engine" console page's state container.</summary>
public abstract class EngineConsoleStateBase : IAsyncDisposable
{
    /// <summary>True while a run is in flight.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last run failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last run's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The last run's formatted result JSON, or null before the first successful run.</summary>
    public string? ResultJson { get; private set; }

    /// <summary>The current, editable request JSON.</summary>
    public string InputJson { get; set; } = string.Empty;

    /// <summary>Sets the console's initial sample request JSON. Called once from the page's OnInitializedAsync.</summary>
    public Task LoadAsync(string sampleInputJson)
    {
        InputJson = sampleInputJson;
        return Task.CompletedTask;
    }

    /// <summary>Runs the engine with the current InputJson via the supplied invocation delegate.</summary>
    public async Task RunAsync(Func<string, Task<string>> invoke)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            ResultJson = await invoke(InputJson);
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

    /// <summary>Resets the console to its empty state.</summary>
    public void Reset()
    {
        ResultJson = null;
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
