// ============================================================================
//  Project:        Deployment
//  Module:         Observability
//  File:           AuditLog.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The immutable, tamper-evident audit
//                  trail every deployment/tenant/config state change is
//                  recorded to. Immutability is enforced two ways: (1) the
//                  backing store is append-only (no update/remove method
//                  exists), and (2) every entry chains to the previous
//                  entry's EntryHash (a classic hash chain, computed via
//                  explicit field concatenation + SHA-256 -- no reflection,
//                  the same technique ConfigSnapshotFactory already uses),
//                  so VerifyChainIntegrity can deterministically detect any
//                  entry that was altered or removed after the fact.
// ============================================================================
using System.Security.Cryptography;
using System.Text;

using Deployment.Governance;

using Shared.Telemetry.Abstractions.Events;

namespace Deployment.Observability;

/// <summary>A single, immutable, hash-chained audit entry.</summary>
public sealed record AuditEntry(
    string EntryId,
    string TenantId,
    long SequenceNumber,
    string Action,
    string ActorDescription,
    string CorrelationId,
    DateTime OccurredAtUtc,
    string PreviousEntryHash,
    string EntryHash);

/// <summary>The immutable, append-only, hash-chained audit trail.</summary>
public sealed class AuditLog
{
    private const string GenesisHash = "GENESIS";

    private readonly object _lock = new();
    private readonly Dictionary<string, List<AuditEntry>> _entriesByTenant = new(StringComparer.Ordinal);

    /// <summary>Appends a new, hash-chained AuditEntry for <paramref name="tenantId"/> and writes a structured log event describing it.</summary>
    public AuditEntry Append(
        string tenantId,
        string action,
        string actorDescription,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(actorDescription);
        ArgumentNullException.ThrowIfNull(correlationId);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        AuditEntry entry;
        lock (_lock)
        {
            if (!_entriesByTenant.TryGetValue(tenantId, out List<AuditEntry>? history))
            {
                history = new List<AuditEntry>();
                _entriesByTenant[tenantId] = history;
            }

            long sequenceNumber = history.Count == 0 ? 1 : history[^1].SequenceNumber + 1;
            string previousEntryHash = history.Count == 0 ? GenesisHash : history[^1].EntryHash;
            string entryId = $"{tenantId}:{sequenceNumber}";
            string entryHash = ComputeEntryHash(entryId, tenantId, sequenceNumber, action, actorDescription, correlationId, occurredAtUtc, previousEntryHash);

            entry = new AuditEntry(entryId, tenantId, sequenceNumber, action, actorDescription, correlationId, occurredAtUtc, previousEntryHash, entryHash);
            history.Add(entry);
        }

        logEventWriter.Write(new LogEvent(
            EventId: GovernanceEvents.AuditEntryAppended,
            Category: LogCategory.Deployment,
            Severity: LogSeverity.Info,
            Message: $"Audit entry appended for tenant \"{tenantId}\": {action} (sequence {entry.SequenceNumber}).",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = tenantId,
                ["Action"] = action,
                ["SequenceNumber"] = entry.SequenceNumber,
                ["EntryHash"] = entry.EntryHash,
            },
            CorrelationId: correlationId,
            Module: "Deployment",
            OccurredAtUtc: occurredAtUtc));

        return entry;
    }

    /// <summary>Returns every audit entry for <paramref name="tenantId"/>, in ascending SequenceNumber order (empty if none).</summary>
    public IReadOnlyList<AuditEntry> GetEntries(string tenantId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        lock (_lock)
        {
            return _entriesByTenant.TryGetValue(tenantId, out List<AuditEntry>? history) ? history.ToArray() : Array.Empty<AuditEntry>();
        }
    }

    /// <summary>
    /// Deterministically recomputes every entry's hash chain for <paramref name="tenantId"/> and
    /// returns true only if every entry's EntryHash matches its recomputed value and every entry's
    /// PreviousEntryHash matches the prior entry's EntryHash.
    /// </summary>
    public bool VerifyChainIntegrity(string tenantId)
    {
        IReadOnlyList<AuditEntry> entries = GetEntries(tenantId);

        string expectedPreviousHash = GenesisHash;
        foreach (AuditEntry entry in entries)
        {
            if (!string.Equals(entry.PreviousEntryHash, expectedPreviousHash, StringComparison.Ordinal))
            {
                return false;
            }

            string recomputedHash = ComputeEntryHash(
                entry.EntryId, entry.TenantId, entry.SequenceNumber, entry.Action, entry.ActorDescription, entry.CorrelationId, entry.OccurredAtUtc, entry.PreviousEntryHash);

            if (!string.Equals(entry.EntryHash, recomputedHash, StringComparison.Ordinal))
            {
                return false;
            }

            expectedPreviousHash = entry.EntryHash;
        }

        return true;
    }

    private static string ComputeEntryHash(
        string entryId, string tenantId, long sequenceNumber, string action, string actorDescription, string correlationId, DateTime occurredAtUtc, string previousEntryHash)
    {
        string canonical = $"{entryId}|{tenantId}|{sequenceNumber}|{action}|{actorDescription}|{correlationId}|{occurredAtUtc:O}|{previousEntryHash}";
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hashBytes);
    }
}
