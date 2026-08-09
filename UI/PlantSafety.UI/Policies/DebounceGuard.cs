// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Policies
//  File:           DebounceGuard.cs
//  Created:        2026-08-09
//  Description:    Phase 2.2 deliverable: part of the UI event policy
//                  engine's debounce rules. Coalesces rapid, repeated
//                  Trigger calls (e.g. every keystroke in a search box)
//                  into a single action that fires only after Delay has
//                  elapsed with no further triggers. The timer callback
//                  always runs on a thread-pool thread, never Blazor's
//                  renderer context, so the action is marshaled back via
//                  IUiDispatcher (Phase 2.2's other deliverable) before
//                  it touches any component state.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Policies;

/// <summary>Coalesces rapid repeated triggers into one delayed action, dispatched back to the UI thread.</summary>
public sealed class DebounceGuard(IUiDispatcher dispatcher, TimeSpan delay) : IDisposable
{
    private readonly Lock _sync = new();
    private Timer? _timer;
    private Func<Task>? _pendingAction;

    /// <summary>Schedules the given action, canceling any not-yet-fired action from a prior Trigger call.</summary>
    public void Trigger(Func<Task> action)
    {
        lock (_sync)
        {
            _pendingAction = action;
            _timer?.Dispose();
            _timer = new Timer(_ => Fire(), null, delay, Timeout.InfiniteTimeSpan);
        }
    }

    private void Fire()
    {
        Func<Task>? action;
        lock (_sync)
        {
            action = _pendingAction;
            _pendingAction = null;
        }

        if (action is not null)
        {
            _ = dispatcher.InvokeAsync(action);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_sync)
        {
            _timer?.Dispose();
        }
    }
}
