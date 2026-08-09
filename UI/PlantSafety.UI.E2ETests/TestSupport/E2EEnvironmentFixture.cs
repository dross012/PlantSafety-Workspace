// ============================================================================
//  Project:        PlantSafety.UI.E2ETests
//  Module:         TestSupport
//  File:           E2EEnvironmentFixture.cs
//  Created:        2026-08-09
//  Description:    Shared, once-per-run test environment: launches real
//                  PlantSafety.UI.Api and PlantSafety.UI.E2EHost processes
//                  (the actual built DLLs, not in-memory test servers) plus
//                  a real headless Chromium instance via Playwright, so the
//                  E2E suite drives the same code paths a live deployment
//                  would. Torn down once after the last test in the
//                  collection completes.
// ============================================================================
using System.Diagnostics;
using System.Net;

using Microsoft.Playwright;

using Xunit;

namespace PlantSafety.UI.E2ETests.TestSupport;

/// <summary>Launches the API + host processes and a headless browser once per test run.</summary>
public sealed class E2EEnvironmentFixture : IAsyncLifetime
{
    private const string ApiBaseUrl = "http://localhost:5329";
    private const string HostBaseUrl = "http://localhost:5330";

    private Process? _apiProcess;
    private Process? _hostProcess;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    /// <summary>The E2EHost's base URL; every page test navigates relative to this.</summary>
    public string HostUrl => HostBaseUrl;

    /// <summary>The shared headless browser instance.</summary>
    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized.");

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        string repoRoot = FindRepoRoot();
        _apiProcess = StartDotnetProject(Path.Combine(repoRoot, "UI", "PlantSafety.UI.Api"), ApiBaseUrl, environment: []);
        await WaitUntilReadyAsync(ApiBaseUrl + "/api/v1/ui/facilities");

        _hostProcess = StartDotnetProject(
            Path.Combine(repoRoot, "UI", "PlantSafety.UI.E2EHost"),
            HostBaseUrl,
            environment: [("ApiBaseAddress", ApiBaseUrl + "/")]);
        await WaitUntilReadyAsync(HostBaseUrl + "/facility/list");

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
        Kill(_hostProcess);
        Kill(_apiProcess);
    }

    private static Process StartDotnetProject(string projectDirectory, string baseUrl, (string Key, string Value)[] environment)
    {
        var startInfo = new ProcessStartInfo("dotnet", $"run --no-build --urls {baseUrl}")
        {
            WorkingDirectory = projectDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            EnvironmentVariables = { ["ASPNETCORE_ENVIRONMENT"] = "Development" },
        };
        foreach ((string key, string value) in environment)
        {
            startInfo.EnvironmentVariables[key] = value;
        }

        var process = new Process { StartInfo = startInfo };
        process.Start();
        return process;
    }

    private static async Task WaitUntilReadyAsync(string url, int timeoutSeconds = 60)
    {
        using var httpClient = new HttpClient();
        DateTime deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"{url} did not become ready within {timeoutSeconds}s.", lastError);
    }

    private static void Kill(Process? process)
    {
        if (process is null || process.HasExited)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "UI")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repo root from " + AppContext.BaseDirectory);
    }
}

/// <summary>xunit collection wiring so every E2E test class shares one environment.</summary>
[CollectionDefinition(Name)]
public sealed class E2ECollection : ICollectionFixture<E2EEnvironmentFixture>
{
    public const string Name = "E2E Environment";
}
