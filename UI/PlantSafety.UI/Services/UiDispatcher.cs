// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           UiDispatcher.cs
//  Created:        2026-08-09
//  Description:    Captures SynchronizationContext.Current at construction
//                  time. Registered Scoped (Section 10 DI convention), a
//                  new instance is created per Blazor circuit/session, and
//                  DI resolves it lazily on first injection -- which
//                  happens while a component is rendering on the Blazor
//                  renderer's own synchronization context, so the capture
//                  is the real UI context, not an arbitrary thread-pool
//                  one. If no synchronization context is present (e.g. a
//                  console test host), actions simply run inline.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>Marshals callbacks onto the Blazor renderer's synchronization context, captured at construction.</summary>
public sealed class UiDispatcher : IUiDispatcher
{
    private readonly SynchronizationContext? _synchronizationContext = SynchronizationContext.Current;

    /// <inheritdoc />
    public Task InvokeAsync(Func<Task> action)
    {
        if (_synchronizationContext is null || SynchronizationContext.Current == _synchronizationContext)
        {
            return action();
        }

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _synchronizationContext.Post(
            async _ =>
            {
                try
                {
                    await action();
                    completion.SetResult();
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            },
            null);
        return completion.Task;
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action) => InvokeAsync(() =>
    {
        action();
        return Task.CompletedTask;
    });
}
