// ============================================================================
//  Project:        Shared.Deterministic.Tests
//  File:           DeterministicRuntimeTests.cs
//  Created:        2026-08-09
//  Description:    Regression tests for the DeterministicRuntime DI
//                  registration -- the deliverable Phase 2.1 asked for:
//                  confirms the deterministic (fixed/seeded) path and the
//                  production (real clock/random) path are wired
//                  correctly and produce reproducible vs. real behavior
//                  respectively.
// ============================================================================
using AwesomeAssertions;

using Microsoft.Extensions.DependencyInjection;

using Shared.Deterministic.Abstractions;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Deterministic.Abstractions.Random;

using Xunit;

namespace Shared.Deterministic.Tests;

public sealed class DeterministicRuntimeTests
{
    [Fact]
    public void AddDeterministicRuntime_DefaultOptions_RegistersSystemImplementations()
    {
        var services = new ServiceCollection();

        services.AddDeterministicRuntime();
        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IClock>().Should().BeOfType<SystemClock>();
        provider.GetRequiredService<IRandomProvider>().Should().BeOfType<SystemRandomProvider>();
    }

    [Fact]
    public void AddDeterministicRuntime_DeterministicModeEnabled_RegistersFixedAndSeededImplementations()
    {
        var services = new ServiceCollection();
        var fixedInstant = new DateTime(2030, 3, 3, 0, 0, 0, DateTimeKind.Utc);

        services.AddDeterministicRuntime(options =>
        {
            options.IsEnabled = true;
            options.FixedUtcNow = fixedInstant;
            options.Seed = 123;
        });
        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IClock>().UtcNow.Should().Be(fixedInstant);
        provider.GetRequiredService<IRandomProvider>().Should().BeOfType<SeededRandomProvider>();
    }

    [Fact]
    public void AddDeterministicRuntime_DeterministicModeEnabled_SameSeedAcrossTwoContainers_ProducesSameGuidSequence()
    {
        var servicesA = new ServiceCollection();
        servicesA.AddDeterministicRuntime(o => { o.IsEnabled = true; o.Seed = 5; });
        var servicesB = new ServiceCollection();
        servicesB.AddDeterministicRuntime(o => { o.IsEnabled = true; o.Seed = 5; });

        IRandomProvider providerA = servicesA.BuildServiceProvider().GetRequiredService<IRandomProvider>();
        IRandomProvider providerB = servicesB.BuildServiceProvider().GetRequiredService<IRandomProvider>();

        providerA.NewGuid().Should().Be(providerB.NewGuid());
    }
}
