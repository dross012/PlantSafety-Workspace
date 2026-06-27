// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Health check registrations for the Application layer.
//   Registers startup validation that all required abstractions are properly wired.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.CurrentUser;

namespace PlantSafety.Application.DependencyInjection.Services;

/// <summary>
/// Registers Application-layer health check validations.
/// Verifies that required Infrastructure implementations are registered before the app starts.
/// </summary>
public static class ApplicationHealthCheckRegistration
{
    /// <summary>
    /// Adds startup validation that required Application abstractions are registered.
    /// Called from the composition root after all services are registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
    {
        // Startup validation: confirm required abstractions are registered by Infrastructure.
        // These throw at startup if the Infrastructure project hasn't wired them up.
        services.AddHostedService<ApplicationAbstractionStartupValidator>();
        return services;
    }
}

/// <summary>
/// Startup validator that asserts required Application abstractions are registered.
/// Throws <see cref="InvalidOperationException"/> at startup if any are missing.
/// </summary>
internal sealed class ApplicationAbstractionStartupValidator
    : Microsoft.Extensions.Hosting.IHostedService
{
    private readonly IServiceProvider _provider;

    /// <summary>Initialises the validator.</summary>
    public ApplicationAbstractionStartupValidator(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();

        // These will throw if not registered — fast fail at startup.
        _ = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        _ = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        _ = scope.ServiceProvider.GetRequiredService<ICacheService>();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
