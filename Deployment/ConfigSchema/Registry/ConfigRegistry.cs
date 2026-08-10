// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Registry
//  File:           ConfigRegistry.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The append-only, immutable-snapshot
//                  config registry: every Publish call adds a new,
//                  strictly-ascending SnapshotVersion for a tenant; no
//                  existing snapshot is ever mutated or removed.
//                  RollbackTo restores a prior version's content by
//                  publishing it again as a brand-new version -- history is
//                  never rewritten, only extended. Thread-safe via a single
//                  lock per registry instance (deterministic ordering of
//                  concurrent Publish calls is not attempted -- callers
//                  needing deterministic ordering across processes must
//                  serialize their own Publish calls, documented honestly
//                  rather than pretending distributed consensus exists here).
// ============================================================================
namespace Deployment.ConfigSchema.Registry;

/// <summary>The append-only, immutable-snapshot config registry.</summary>
public sealed class ConfigRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<string, List<ConfigSnapshot>> _snapshotsByTenant = new(StringComparer.Ordinal);

    /// <summary>
    /// Publishes <paramref name="snapshot"/>. Throws InvalidOperationException if
    /// <paramref name="snapshot"/>.SnapshotVersion is not exactly one greater than the tenant's
    /// current latest version (or 1, if this is the tenant's first snapshot).
    /// </summary>
    public ConfigSnapshot Publish(ConfigSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        lock (_lock)
        {
            if (!_snapshotsByTenant.TryGetValue(snapshot.TenantId, out List<ConfigSnapshot>? history))
            {
                history = new List<ConfigSnapshot>();
                _snapshotsByTenant[snapshot.TenantId] = history;
            }

            int expectedVersion = history.Count == 0 ? 1 : history[^1].SnapshotVersion + 1;
            if (snapshot.SnapshotVersion != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Cannot publish SnapshotVersion {snapshot.SnapshotVersion} for tenant \"{snapshot.TenantId}\": expected {expectedVersion} (strictly ascending, append-only).");
            }

            history.Add(snapshot);
            return snapshot;
        }
    }

    /// <summary>Returns the latest published snapshot for <paramref name="tenantId"/>, or null if none exists.</summary>
    public ConfigSnapshot? GetLatest(string tenantId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        lock (_lock)
        {
            return _snapshotsByTenant.TryGetValue(tenantId, out List<ConfigSnapshot>? history) && history.Count > 0
                ? history[^1]
                : null;
        }
    }

    /// <summary>Returns the snapshot for <paramref name="tenantId"/> at <paramref name="version"/>, or null if it does not exist.</summary>
    public ConfigSnapshot? GetVersion(string tenantId, int version)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        lock (_lock)
        {
            return _snapshotsByTenant.TryGetValue(tenantId, out List<ConfigSnapshot>? history)
                ? history.FirstOrDefault(s => s.SnapshotVersion == version)
                : null;
        }
    }

    /// <summary>Returns every published snapshot for <paramref name="tenantId"/>, in ascending SnapshotVersion order (empty if none).</summary>
    public IReadOnlyList<ConfigSnapshot> ListVersions(string tenantId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        lock (_lock)
        {
            return _snapshotsByTenant.TryGetValue(tenantId, out List<ConfigSnapshot>? history)
                ? history.ToArray()
                : Array.Empty<ConfigSnapshot>();
        }
    }

    /// <summary>
    /// Restores <paramref name="targetVersion"/>'s content for <paramref name="tenantId"/> by publishing
    /// a brand-new snapshot (at the next ascending version) carrying that content, using
    /// <paramref name="createdAtUtc"/> as its timestamp. History is never rewritten. Throws
    /// InvalidOperationException if <paramref name="targetVersion"/> does not exist.
    /// </summary>
    public ConfigSnapshot RollbackTo(string tenantId, int targetVersion, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        ConfigSnapshot? target = GetVersion(tenantId, targetVersion);
        if (target is null)
        {
            throw new InvalidOperationException($"Cannot roll back tenant \"{tenantId}\" to version {targetVersion}: that version does not exist.");
        }

        lock (_lock)
        {
            List<ConfigSnapshot> history = _snapshotsByTenant[tenantId];
            int nextVersion = history[^1].SnapshotVersion + 1;

            ConfigSnapshot rolledBack = ConfigSnapshotFactory.Create(
                tenantId, nextVersion, target.DeploymentConfig, target.TenantConfigs, target.RoleConfigs, target.ModuleConfigs, createdAtUtc);

            history.Add(rolledBack);
            return rolledBack;
        }
    }
}
