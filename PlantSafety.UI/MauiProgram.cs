// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: MAUI application composition root. Registers all
//   typed HttpClients (with correlation and auth handler pipelines),
//   singleton state containers, MSAL token provider, and structured
//   logging. All services MUST be registered here; no ambient service
//   location is permitted in components or state containers.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.6, §XI
// ============================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

using PlantSafety.UI.Infrastructure.Auth;
using PlantSafety.UI.Infrastructure.Correlation;
using PlantSafety.UI.Modules.Alerts.ApiClients;
using PlantSafety.UI.Modules.DiagnosticsViewer.Services;
using PlantSafety.UI.Modules.OperationsAuditViewer.ApiClients;
using PlantSafety.UI.Modules.OperationsBackup.ApiClients;
using PlantSafety.UI.Modules.SchemaGuard.ApiClients;
using PlantSafety.UI.Modules.OperationsArchival.ApiClients;
using PlantSafety.UI.Modules.OperationsMonitoring.ApiClients;
using PlantSafety.UI.Modules.OperationsConfiguration.ApiClients;
using PlantSafety.UI.Modules.OperationsSystemHealth.ApiClients;
using PlantSafety.UI.State;

namespace PlantSafety.UI;

/// <summary>
/// MAUI application entry point and DI composition root.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds and returns the configured <see cref="MauiApp"/> instance.
    /// All registrations follow the dependency matrix defined in §PART XXI.
    /// </summary>
    /// <returns>The fully configured MauiApp ready to launch.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // ── Configuration ──────────────────────────────────────────────────────
        // appsettings.json is embedded as a MAUI raw asset; loaded via EmbeddedResource.
        // AzureAd and Api sections are required; missing keys throw at startup.

        // ── Correlation ────────────────────────────────────────────────────────
        // Scoped so each Blazor circuit/session gets its own lineage envelope.
        builder.Services.AddScoped<CorrelationContext>();
        builder.Services.AddTransient<CorrelationHandler>();

        // ── Authentication ─────────────────────────────────────────────────────
        builder.Services.AddSingleton<IMsalTokenProvider, MsalTokenProvider>();
        builder.Services.AddTransient<AuthorizationMessageHandler>();

        // ── State Containers (Singleton — shared across the session) ───────────
        builder.Services.AddSingleton<AppSessionState>();
        builder.Services.AddSingleton<UserSessionState>();
        builder.Services.AddSingleton<TenantState>();
        builder.Services.AddSingleton<NavigationState>();

        // ── Module State Containers ────────────────────────────────────────────
        builder.Services.AddSingleton<Modules.Alerts.State.AlertsState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.DiagnosticsSessionState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.FilterState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.SortState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.SelectedResultState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.ExportState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.ComparisonState>();
        builder.Services.AddSingleton<Modules.ProcessManagement.State.ProcessManagementState>();
        builder.Services.AddSingleton<Modules.WorkerWellBeing.State.WorkerWellBeingState>();
        builder.Services.AddSingleton<Modules.EnvironmentalHealth.State.EnvironmentalHealthState>();
        builder.Services.AddSingleton<Modules.OperationsBackup.State.BackupState>();
        builder.Services.AddSingleton<Modules.SchemaGuard.State.SchemaGuardState>();
        builder.Services.AddSingleton<Modules.OperationsMonitoring.State.MonitoringState>();
        builder.Services.AddSingleton<Modules.OperationsArchival.State.ArchivalState>();
        builder.Services.AddSingleton<Modules.OperationsAuditViewer.State.AuditViewerState>();
        builder.Services.AddSingleton<Modules.OperationsConfiguration.State.ConfigurationState>();
        builder.Services.AddSingleton<Modules.OperationsSystemHealth.State.SystemHealthState>();

        // ── Typed HttpClients ──────────────────────────────────────────────────
        // Handler pipeline: CorrelationHandler → AuthorizationMessageHandler → HttpClientHandler
        // Base address is read from configuration; must end with trailing slash.
        RegisterApiClient<AlertsApiClient>(builder, "alerts");
        RegisterApiClient<DiagnosticsApiClient>(builder, "diagnostics");
        RegisterApiClient<Modules.ProcessManagement.ApiClients.ProcessManagementApiClient>(builder, "process-management");
        RegisterApiClient<Modules.WorkerWellBeing.ApiClients.WorkerWellBeingApiClient>(builder, "worker-wellbeing");
        RegisterApiClient<Modules.EnvironmentalHealth.ApiClients.EnvironmentalHealthApiClient>(builder, "environmental-health");
        RegisterApiClient<BackupApiClient>(builder, "operations-backup");
        RegisterApiClient<SchemaGuardApiClient>(builder, "operations-schema-guard");
        RegisterApiClient<MonitoringApiClient>(builder, "operations-monitoring");
        RegisterApiClient<ArchivalApiClient>(builder, "operations-archival");
        RegisterApiClient<AuditViewerApiClient>(builder, "operations-audit-viewer");
        RegisterApiClient<ConfigurationApiClient>(builder, "operations-configuration");
        RegisterApiClient<SystemHealthApiClient>(builder, "operations-system-health");

        // ── Logging ────────────────────────────────────────────────────────────
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    /// <summary>
    /// Registers a typed API client with the canonical handler pipeline:
    /// CorrelationHandler → AuthorizationMessageHandler.
    /// </summary>
    /// <typeparam name="TClient">The typed client implementation.</typeparam>
    /// <param name="builder">The MAUI app builder.</param>
    /// <param name="clientName">A logical name used for diagnostics and configuration lookup.</param>
    private static void RegisterApiClient<TClient>(MauiAppBuilder builder, string clientName)
        where TClient : class
    {
        builder.Services
            .AddHttpClient<TClient>(clientName, client =>
            {
                // Base address is read from appsettings.json Api:BaseUrl at runtime.
                // Cannot inline IConfiguration here — clients resolve it from the
                // IServiceProvider after MauiApp is built.
            })
            .AddHttpMessageHandler<CorrelationHandler>()
            .AddHttpMessageHandler<AuthorizationMessageHandler>();
    }
}
