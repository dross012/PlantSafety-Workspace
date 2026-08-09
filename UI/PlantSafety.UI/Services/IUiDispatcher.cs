// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           IUiDispatcher.cs
//  Created:        2026-08-09
//  Description:    Phase 2.2 (Blazor Hybrid UI Stabilization): the one
//                  approved seam for marshaling a callback back onto the
//                  Blazor renderer's synchronization context. Blazor
//                  component lifecycle methods and @onclick handlers
//                  already run on that context, so IUiDispatcher only
//                  matters for code that resumes off it -- most notably
//                  a System.Threading.Timer callback (see DebounceGuard),
//                  which always fires on a thread-pool thread and would
//                  otherwise corrupt Blazor's render tree if it touched
//                  component state directly.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>The one approved seam for marshaling a callback onto the UI's synchronization context.</summary>
public interface IUiDispatcher
{
    /// <summary>Runs the given action on the UI thread, awaiting its completion.</summary>
    Task InvokeAsync(Func<Task> action);

    /// <summary>Runs the given synchronous action on the UI thread, awaiting its completion.</summary>
    Task InvokeAsync(Action action);
}
