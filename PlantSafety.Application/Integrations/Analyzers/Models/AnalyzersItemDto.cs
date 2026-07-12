// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single automated integration analyzer run result.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Analyzers.Models;

/// <summary>A single result produced by an automated integration health/data-quality analyzer run.</summary>
public sealed record AnalyzersItemDto
{
    /// <summary>Analyzer run identifier.</summary>
    public required Guid RunId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the analyzer that executed.</summary>
    public required string AnalyzerName { get; init; }

    /// <summary>External system the analyzer evaluated.</summary>
    public required string TargetSystem { get; init; }

    /// <summary>Number of findings produced by the analyzer run.</summary>
    public required int FindingsCount { get; init; }

    /// <summary>Outcome status of the analyzer run (e.g., Passed/Warning/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the analyzer run completed.</summary>
    public required DateTime RunAtUtc { get; init; }

    /// <summary>Identity of the user or scheduler that triggered the run.</summary>
    public required Guid TriggeredBy { get; init; }
}
