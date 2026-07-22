// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared query-string request DTO bound to every SafetyPolicies category
//   read-model GET action (one per PolicyCategory). Mirrors the shared shape of every
//   Get<Category>Query in the Application layer: TenantId (from operation context, not bound),
//   optional SiteId filter, and an inclusive UTC date range.
// SchemaVersion: 1

namespace PlantSafety.Api.SafetyPolicies.Contracts.Requests;

/// <summary>Query-string filters shared by all SafetyPolicies category read-model endpoints.</summary>
public sealed class CategoryQueryRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the UTC date range.</summary>
    public DateTime From { get; init; }

    /// <summary>Inclusive end of the UTC date range.</summary>
    public DateTime To { get; init; }
}
