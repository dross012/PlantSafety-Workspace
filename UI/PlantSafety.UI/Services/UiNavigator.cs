// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           UiNavigator.cs
//  Created:        2026-08-09
//  Description:    The production IUiNavigator: a thin wrapper over
//                  Blazor's real NavigationManager.
// ============================================================================
using Microsoft.AspNetCore.Components;

namespace PlantSafety.UI.Services;

/// <summary>The production navigator: delegates to Blazor's real NavigationManager.</summary>
public sealed class UiNavigator(NavigationManager navigationManager) : IUiNavigator
{
    /// <inheritdoc />
    public void NavigateTo(string uri) => navigationManager.NavigateTo(uri);
}
