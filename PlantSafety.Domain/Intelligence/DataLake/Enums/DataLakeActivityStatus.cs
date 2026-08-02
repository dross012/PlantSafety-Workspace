// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for an Intelligence.DataLake DataLakeActivity record. Phase 4
//                  full build-out: this bounded context previously had no aggregate at all -- only
//                  a placeholder marker class (DataLakeModuleMarker) and a read-only date-range
//                  list query (GetDataLakeQuery/DataLakeDapperRepository over
//                  usp_DataLake_GetDataLakeRecords) existed. Values Ingested/Processing/Curated/
//                  Failed are taken verbatim from the vocabulary already documented on the
//                  pre-existing PlantSafety.Application.Intelligence.DataLake.Models
//                  .DataLakeItemDto.Status ("e.g., Ingested/Processing/Curated/Failed") -- no new
//                  states were invented; Archived is added as the platform's standard terminal
//                  retention state, consistent with every other Phase 4 aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.DataLake.Enums;

/// <summary>Lifecycle states for a <see cref="Aggregates.DataLakeActivity"/> record.</summary>
public enum DataLakeActivityStatus
{
    /// <summary>Data has been ingested into the lake but not yet processed. Initial state on Create.</summary>
    Ingested = 0,

    /// <summary>The activity is actively processing the ingested data.</summary>
    Processing = 1,

    /// <summary>Processing completed successfully; the data has been curated.</summary>
    Curated = 2,

    /// <summary>Processing failed.</summary>
    Failed = 3,

    /// <summary>The activity has been archived for compliance retention.</summary>
    Archived = 4,
}
