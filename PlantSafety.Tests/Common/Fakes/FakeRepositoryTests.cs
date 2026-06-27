// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.Fakes
//  Layer:          Tests (Unit)
//  File:           FakeRepositoryTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests verifying FakeRepository<TAggregate, TId> in-memory behavior.
// ============================================================================
using PlantSafety.Tests.Common.Fakes;

namespace PlantSafety.Tests.Common.Fakes;

public sealed class FakeRepositoryTests : UnitTestBase
{
    private readonly FakeRepository<Alert, AlertId> _repo = new();

    private Alert CreateAlert() =>
        Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

    [Fact]
    public async Task AddAsync_ShouldStoreAggregate()
    {
        var agg = CreateAlert();
        await _repo.AddAsync(agg, CancellationToken.None);
        var found = await _repo.GetByIdAsync(agg.AlertId, CancellationToken.None);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ShouldReturnNull()
    {
        var found = await _repo.GetByIdAsync(AlertId.New(), CancellationToken.None);
        found.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_MultipleTimes_ShouldStoreAll()
    {
        await _repo.AddAsync(CreateAlert(), CancellationToken.None);
        await _repo.AddAsync(CreateAlert(), CancellationToken.None);
        _repo.All.Should().HaveCount(2);
    }

    [Fact]
    public async Task Clear_ShouldEmptyRepository()
    {
        await _repo.AddAsync(CreateAlert(), CancellationToken.None);
        _repo.Clear();
        _repo.All.Should().BeEmpty();
    }
}
