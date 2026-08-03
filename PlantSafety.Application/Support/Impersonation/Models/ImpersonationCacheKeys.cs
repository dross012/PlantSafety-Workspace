// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Shared ICacheService key builder for impersonation sessions, so
//   Start/End/Get handlers never drift on the key format.
// SchemaVersion: 1

namespace PlantSafety.Application.Support.Impersonation.Models;

/// <summary>Builds the canonical <see cref="Abstractions.Caching.ICacheService"/> key for an impersonation session.</summary>
public static class ImpersonationCacheKeys
{
    private const string Prefix = "support:impersonation:session:";

    /// <summary>Builds the cache key for the given session identity.</summary>
    public static string ForSession(Guid sessionId) => Prefix + sessionId;
}
