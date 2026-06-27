// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Startup validator that asserts required Application-layer abstractions are
//   registered in the DI container.  Runs once at startup; throws if any required service is absent.
//   This prevents silent misconfiguration where handlers fail only at runtime.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;

namespace PlantSafety.Application.DependencyInjection.Validation;

/// <summary>
/// Validates that mandatory Application-layer service registrations exist at startup.
/// Call <see cref="ValidateAsync"/> from the host startup pipeline after the DI container is built.
/// </summary>
public sealed class DiRegistrationValidator
{
    private static readonly Type[] RequiredServices =
    [
        typeof(IDateTimeProvider),
    ];

    private readonly IServiceProvider _provider;
    private readonly ILogger<DiRegistrationValidator> _logger;

    /// <summary>Initialises the validator.</summary>
    public DiRegistrationValidator(IServiceProvider provider, ILogger<DiRegistrationValidator> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Checks that every required service type resolves from the container.
    /// Throws <see cref="InvalidOperationException"/> listing all missing registrations.
    /// </summary>
    public Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        var missing = new List<string>();

        foreach (var serviceType in RequiredServices)
        {
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider.GetService(serviceType);
            if (service is null)
            {
                missing.Add(serviceType.FullName ?? serviceType.Name);
                _logger.LogError("Required service {ServiceType} is not registered in the DI container.", serviceType.FullName);
            }
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"PlantSafety.Application startup validation failed. The following required services are not registered: {string.Join(", ", missing)}");
        }

        _logger.LogInformation("DiRegistrationValidator: all required Application services are registered.");
        return Task.CompletedTask;
    }
}
