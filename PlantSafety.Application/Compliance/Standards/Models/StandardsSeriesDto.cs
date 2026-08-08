// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.StandardsSeries row (e.g. ISO
//   9001, ASME B31, ANSI Z87). Global reference data, not tenant-scoped.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Standards.Models;

/// <summary>Flat read-model DTO for a single Compliance.StandardsSeries row.</summary>
public sealed record StandardsSeriesDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid SeriesId { get; init; }

    /// <summary>The standards family this series belongs to (e.g. "ISO").</summary>
    public required string FamilyCode { get; init; }

    /// <summary>Short series code (e.g. "9001", "B31", "Z87").</summary>
    public required string SeriesCode { get; init; }

    /// <summary>Full series name.</summary>
    public required string SeriesName { get; init; }

    /// <summary>Narrative description of the series' scope.</summary>
    public string? Description { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
