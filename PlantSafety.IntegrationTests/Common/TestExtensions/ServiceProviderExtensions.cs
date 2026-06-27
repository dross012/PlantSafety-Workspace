// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           ServiceProviderExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Extension methods that resolve scoped services from the test DI container.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.TestExtensions;

/// <summary>
/// Extension methods for resolving services from the test <see cref="IServiceProvider"/>
/// within integration tests.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates a new DI scope and resolves the requested service.
    /// The caller is responsible for disposing the scope.
    /// </summary>
    public static (IServiceScope Scope, T Service) GetScopedService<T>(
        this IServiceProvider serviceProvider) where T : notnull
    {
        var scope   = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        return (scope, service);
    }
}
