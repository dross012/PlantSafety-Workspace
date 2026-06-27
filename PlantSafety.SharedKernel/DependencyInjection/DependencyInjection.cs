/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               DependencyInjection.cs
    DESCRIPTION:        Canonical SharedKernel dependency injection entry point. Provided for architectural symmetry and
                        onboarding clarity. SharedKernel defines abstractions and primitives only and MUST NOT register
                        concrete infrastructure implementations. This extension exists so that all modules can follow the
                        same "AddXyz" pattern without leaking infrastructure concerns into SharedKernel.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New optional configuration hooks MAY be added; existing behavior MUST remain
                        a no-op with respect to concrete implementations.
==========================================================================================================================*/

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.SharedKernel.DependencyInjection;

/// <summary>
/// Canonical SharedKernel dependency injection entry point.
/// <para>
/// SharedKernel:
/// - Defines abstractions, primitives, and contracts only.
/// - MUST NOT register concrete implementations.
/// - MUST remain infrastructure-agnostic.
/// </para>
/// <para>
/// This method exists for architectural symmetry:
/// - API / Application / Infrastructure call AddSharedKernel() just like other modules.
/// - SafetyBuild / SafetyAnalyzer can rely on a consistent DI pattern.
/// </para>
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers SharedKernel-related services into the DI container.
    /// <para>
    /// Current behavior:
    /// - No concrete services are registered.
    /// - Method exists as a stable extension point and documentation anchor.
    /// </para>
    /// <para>
    /// Future evolution (additive-only):
    /// - MAY register analyzers, validators, or configuration objects that are still
    ///   abstraction-only and do not introduce infrastructure dependencies.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for fluent chaining.</returns>
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        // Intentionally no-op for now.
        // SharedKernel MUST remain free of concrete infrastructure registrations.
        return services;
    }
}
