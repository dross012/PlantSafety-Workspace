// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Data Lake module. Mirrors the real
//   PlantSafety.Api read-model shapes (DataLakeItemDto / DataLakeResponse at
//   GET v1/intelligence/datalake). Zone is one of the 5 canonical data lake
//   zones (Raw/Cleansed/Curated/ML/Sandbox) per the DataLake Design & Rules
//   Manuals.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.DataLake.Models;

/// <summary>Represents a single data lake ingestion/curation activity record (mirrors <c>DataLakeItemDto</c>).</summary>
public sealed class DataLakeItemDto
{
    /// <summary>Gets the unique identifier of the data lake activity record.</summary>
    public Guid DataLakeRecordId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the name of the dataset associated with this record.</summary>
    public string DatasetName { get; init; } = string.Empty;

    /// <summary>Gets the data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>Gets the number of records processed in this activity.</summary>
    public long? RecordCount { get; init; }

    /// <summary>Gets the current activity status (e.g., Ingested/Processing/Curated/Failed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the activity was processed.</summary>
    public DateTime ProcessedAtUtc { get; init; }

    /// <summary>Gets the identity of the user or process that performed the activity.</summary>
    public Guid ProcessedBy { get; init; }
}

/// <summary>Envelope returned by GET v1/intelligence/datalake (mirrors <c>DataLakeResponse</c>).</summary>
public sealed class DataLakeResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the data lake activity items in the requested window.</summary>
    public IReadOnlyList<DataLakeItemDto> Items { get; init; } = [];
}
