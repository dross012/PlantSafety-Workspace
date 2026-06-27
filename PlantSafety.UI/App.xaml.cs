// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: MAUI Application class. Sets the MainPage to the
//   Blazor WebView host page. Auth initialization is delegated to the
//   AppShell Blazor component to keep platform and Blazor concerns separated.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.6
// ============================================================

namespace PlantSafety.UI;

/// <summary>
/// MAUI application host class. Responsible only for setting the root page.
/// All session initialization, auth, and state loading occur inside the
/// Blazor component tree via the AppShell component.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the MAUI app and sets the root page to the Blazor WebView host.
    /// </summary>
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }
}
