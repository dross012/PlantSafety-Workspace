// ============================================================================
//  Project:        Shared.SnapshotTesting
//  File:           SnapshotMismatchException.cs
//  Created:        2026-08-09
//  Description:    Phase 2.10 (Regression Safety Net): thrown when a
//                  snapshot test's actual output doesn't match its golden
//                  file. Carries both values so the test runner's failure
//                  message shows the real diff, not just "mismatch."
// ============================================================================
namespace Shared.SnapshotTesting;

/// <summary>Thrown when a snapshot's actual output doesn't match its golden file.</summary>
public sealed class SnapshotMismatchException(string snapshotName, string reason, string expected, string actual)
    : Exception(BuildMessage(snapshotName, reason, expected, actual))
{
    /// <summary>The snapshot's name.</summary>
    public string SnapshotName { get; } = snapshotName;

    /// <summary>The golden file's content (empty if none existed yet).</summary>
    public string Expected { get; } = expected;

    /// <summary>The actual output that was compared against the golden file.</summary>
    public string Actual { get; } = actual;

    private static string BuildMessage(string snapshotName, string reason, string expected, string actual) =>
        $"Snapshot \"{snapshotName}\" mismatch: {reason}{Environment.NewLine}" +
        $"--- Expected (golden file) ---{Environment.NewLine}{expected}{Environment.NewLine}" +
        $"--- Actual ---{Environment.NewLine}{actual}";
}
