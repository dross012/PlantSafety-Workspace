// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions
//  File:           DeterministicRuntime.cs
//  Created:        2026-08-09
//  Description:    Phase 2.1 deliverable: the deterministic runtime layer.
//                  One DI extension method every host (PlantSafety.UI.Api,
//                  any future host) calls once at startup. Registers the
//                  real SystemClock/SystemRandomProvider by default;
//                  registers FixedClock/SeededRandomProvider instead when
//                  DeterministicModeOptions.IsEnabled is true (set via
//                  configuration or an integration test's WebApplicationFactory
//                  override), so a whole test run gets reproducible
//                  timestamps and identities without each test hand-wiring
//                  its own fakes.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Deterministic.Abstractions.Mode;
using Shared.Deterministic.Abstractions.Random;

namespace Shared.Deterministic.Abstractions;

/// <summary>Registers the deterministic runtime (IClock/IRandomProvider) for a host's DI container.</summary>
public static class DeterministicRuntime
{
    /// <summary>
    /// Registers IClock and IRandomProvider. When <paramref name="configureMode"/> leaves
    /// DeterministicModeOptions.IsEnabled false (the default), registers the real
    /// SystemClock/SystemRandomProvider. When true, registers FixedClock (pinned to
    /// FixedUtcNow) and SeededRandomProvider (pinned to Seed) instead.
    /// </summary>
    public static IServiceCollection AddDeterministicRuntime(this IServiceCollection services, Action<DeterministicModeOptions>? configureMode = null)
    {
        var options = new DeterministicModeOptions();
        configureMode?.Invoke(options);

        if (options.IsEnabled)
        {
            services.AddSingleton<IClock>(new FixedClock(options.FixedUtcNow));
            services.AddSingleton<IRandomProvider>(new SeededRandomProvider(options.Seed));
        }
        else
        {
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<IRandomProvider, SystemRandomProvider>();
        }

        return services;
    }
}
