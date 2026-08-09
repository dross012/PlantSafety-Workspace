// ============================================================================
//  Project:        Shared.SnapshotTesting.Tests
//  File:           SnapshotHarnessTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Xunit;

namespace Shared.SnapshotTesting.Tests;

public sealed class SnapshotHarnessTests : IDisposable
{
    private readonly string _snapshotDirectory = Path.Combine(Path.GetTempPath(), "ps-snapshot-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_snapshotDirectory))
        {
            Directory.Delete(_snapshotDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_NoGoldenFileAndNotBaselineMode_ThrowsSnapshotMismatchException()
    {
        Func<Task> act = () => SnapshotHarness.VerifyAsync(_snapshotDirectory, "missing", """{"a":1}""", baselineModeOverride: false);

        await act.Should().ThrowAsync<SnapshotMismatchException>();
    }

    [Fact]
    public async Task VerifyAsync_BaselineMode_WritesTheGoldenFile()
    {
        await SnapshotHarness.VerifyAsync(_snapshotDirectory, "new-snapshot", """{"a":1}""", baselineModeOverride: true);

        File.Exists(Path.Combine(_snapshotDirectory, "new-snapshot.json")).Should().BeTrue();
    }

    [Fact]
    public async Task VerifyAsync_MatchingActualAgainstGoldenFile_DoesNotThrow()
    {
        await SnapshotHarness.VerifyAsync(_snapshotDirectory, "matching", """{"a":1,"b":"x"}""", baselineModeOverride: true);

        Func<Task> act = () => SnapshotHarness.VerifyAsync(_snapshotDirectory, "matching", """{"a":1,"b":"x"}""", baselineModeOverride: false);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_DifferentFormattingButSameJsonContent_StillMatches()
    {
        await SnapshotHarness.VerifyAsync(_snapshotDirectory, "reformatted", """{"a":1,"b":"x"}""", baselineModeOverride: true);

        Func<Task> act = () => SnapshotHarness.VerifyAsync(_snapshotDirectory, "reformatted", "{\n  \"a\": 1,\n  \"b\": \"x\"\n}", baselineModeOverride: false);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_ActualDiffersFromGoldenFile_ThrowsWithBothValues()
    {
        await SnapshotHarness.VerifyAsync(_snapshotDirectory, "changed", """{"a":1}""", baselineModeOverride: true);

        Func<Task> act = () => SnapshotHarness.VerifyAsync(_snapshotDirectory, "changed", """{"a":2}""", baselineModeOverride: false);

        (await act.Should().ThrowAsync<SnapshotMismatchException>())
            .Which.Actual.Should().Contain("2");
    }

    [Fact]
    public void BaselineMode_DefaultsToFalse_WhenEnvironmentVariableNotSet()
    {
        Environment.SetEnvironmentVariable("SNAPSHOT_BASELINE", null);

        SnapshotHarness.BaselineMode.Should().BeFalse();
    }
}
