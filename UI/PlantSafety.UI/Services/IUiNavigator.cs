// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           IUiNavigator.cs
//  Created:        2026-08-09
//  Description:    Phase 2.2 deliverable: deterministic navigation rules.
//                  The one approved seam for page-triggered navigation --
//                  pages inject IUiNavigator instead of NavigationManager
//                  directly, formalizing the "only pages navigate, never
//                  a UiService/State container" rule that
//                  Components/Routes.razor and ModuleNavComponent.razor
//                  already documented in comments but never actually
//                  enforced through a seam. A bUnit test can substitute a
//                  fake IUiNavigator to assert "this action navigates to
//                  X" without depending on bUnit's own fake
//                  NavigationManager plumbing.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>The one approved seam for page-triggered navigation.</summary>
public interface IUiNavigator
{
    /// <summary>Navigates to the given relative or absolute URI.</summary>
    void NavigateTo(string uri);
}
