// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiSafetyCopilot.State
//  File:           AiSafetyCopilotDashboardState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Safety Copilot dashboard page.
// ============================================================================
using PlantSafety.UI.Modules.AiSafetyCopilot.Services;

namespace PlantSafety.UI.Modules.AiSafetyCopilot.State;

/// <summary>State container for the AI Safety Copilot dashboard page.</summary>
public sealed class AiSafetyCopilotDashboardState(AiSafetyCopilotUiService uiService) : IAsyncDisposable
{
    /// <summary>True while a guidance request is in flight.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last request failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last request's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The copilot's last guidance output, or null before the first request.</summary>
    public SafetyCopilotOutputDto? Guidance { get; private set; }

    /// <summary>Requests tailored guidance for the given request.</summary>
    public async Task RequestGuidanceAsync(SafetyCopilotGuidanceRequest request)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Guidance = await uiService.GuideAsync(request);
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
        Guidance = null;
        HasError = false;
        Error = null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
