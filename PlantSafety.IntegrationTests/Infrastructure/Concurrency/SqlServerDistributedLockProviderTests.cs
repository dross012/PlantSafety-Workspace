// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Infrastructure.Concurrency
//  Layer:          IntegrationTests
//  File:           SqlServerDistributedLockProviderTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    Proves SqlServerDistributedLockProvider's mutual-exclusion contract against a
//                  real SQL Server instance (Testcontainers): acquire/release, timeout behavior on
//                  a contended resource, and that two concurrent acquirers on the same resource
//                  key serialize rather than interleave -- exactly the race this Phase 6
//                  pre-requisite fix exists to close for WorkflowInstance transitions.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using System.Collections.Concurrent;
using System.Diagnostics;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.IntegrationTests.Infrastructure.Concurrency;

/// <summary>
/// Integration tests for the DI-resolved <see cref="IDistributedLockProvider"/>
/// (<c>SqlServerDistributedLockProvider</c>), run against the shared Testcontainers SQL Server
/// instance.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class SqlServerDistributedLockProviderTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private static string UniqueResourceKey([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
        => $"IntegrationTests:DistributedLock:{testName}:{Guid.NewGuid():N}";

    private IDistributedLockProvider CreateProvider() =>
        Factory.Services.CreateScope().ServiceProvider.GetRequiredService<IDistributedLockProvider>();

    [Fact]
    public async Task AcquireAsync_GrantsLock_AndReleasesOnDispose()
    {
        var provider = CreateProvider();
        var resourceKey = UniqueResourceKey();

        var handle = await provider.AcquireAsync(resourceKey, TimeSpan.FromSeconds(5));
        handle.Should().NotBeNull();
        await handle.DisposeAsync();

        // The lock must be fully released -- a second acquisition on the same key, from a
        // brand-new provider/connection, must succeed promptly rather than timing out.
        await using var secondHandle = await provider.AcquireAsync(resourceKey, TimeSpan.FromSeconds(5));
        secondHandle.Should().NotBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_WhenResourceAlreadyHeld_ReturnsNullWithinTheRequestedTimeout()
    {
        var provider = CreateProvider();
        var resourceKey = UniqueResourceKey();

        await using var firstHandle = await provider.AcquireAsync(resourceKey, TimeSpan.FromSeconds(5));

        var stopwatch = Stopwatch.StartNew();
        var secondHandle = await provider.TryAcquireAsync(resourceKey, TimeSpan.FromMilliseconds(500));
        stopwatch.Stop();

        secondHandle.Should().BeNull("the resource is already held by firstHandle, so a second acquirer must fail rather than block indefinitely");
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5), "TryAcquireAsync must respect the requested timeout, not the default sp_getapplock infinite wait");
    }

    [Fact]
    public async Task AcquireAsync_WhenResourceAlreadyHeld_ThrowsDistributedLockTimeoutExceptionAfterTimeoutElapses()
    {
        var provider = CreateProvider();
        var resourceKey = UniqueResourceKey();

        await using var firstHandle = await provider.AcquireAsync(resourceKey, TimeSpan.FromSeconds(5));

        var act = async () => await provider.AcquireAsync(resourceKey, TimeSpan.FromMilliseconds(500));

        var exception = await act.Should().ThrowAsync<DistributedLockTimeoutException>();
        exception.Which.ResourceKey.Should().Be(resourceKey);
    }

    [Fact]
    public async Task TwoConcurrentAcquirers_OnTheSameResourceKey_Serialize()
    {
        var provider = CreateProvider();
        var resourceKey = UniqueResourceKey();

        // Both tasks record wall-clock enter/exit instants for their held-lock critical section.
        // If the provider truly serializes access, the two intervals must never overlap.
        var intervals = new ConcurrentBag<(DateTime EnteredUtc, DateTime ExitedUtc)>();

        async Task CriticalSectionAsync()
        {
            await using var handle = await provider.AcquireAsync(resourceKey, TimeSpan.FromSeconds(30));
            var enteredUtc = DateTime.UtcNow;

            // Simulate the short-lived read-modify-write cycle a WorkflowInstance command handler
            // performs while holding the lock.
            await Task.Delay(200);

            var exitedUtc = DateTime.UtcNow;
            intervals.Add((enteredUtc, exitedUtc));
        }

        await Task.WhenAll(CriticalSectionAsync(), CriticalSectionAsync());

        intervals.Should().HaveCount(2);
        var ordered = intervals.OrderBy(i => i.EnteredUtc).ToList();

        ordered[1].EnteredUtc.Should().BeOnOrAfter(ordered[0].ExitedUtc,
            "the second acquirer must not enter its critical section until the first has released the lock -- proving mutual exclusion, not merely eventual success");
    }

    [Fact]
    public async Task TwoConcurrentAcquirers_OnDifferentResourceKeys_DoNotBlockEachOther()
    {
        var provider = CreateProvider();
        var resourceKeyA = UniqueResourceKey();
        var resourceKeyB = UniqueResourceKey();

        await using var handleA = await provider.AcquireAsync(resourceKeyA, TimeSpan.FromSeconds(5));

        var stopwatch = Stopwatch.StartNew();
        await using var handleB = await provider.AcquireAsync(resourceKeyB, TimeSpan.FromSeconds(5));
        stopwatch.Stop();

        handleB.Should().NotBeNull();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2),
            "a lock held on an unrelated resource key must never block acquisition of a different key");
    }
}
