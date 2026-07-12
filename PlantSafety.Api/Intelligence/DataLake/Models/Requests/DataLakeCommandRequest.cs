// ============================================================================
// File: DataLakeCommandRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for DataLake queries and commands.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from validated Contracts/Requests DTOs. Passed to Application-layer query/command constructors.
//
// Rationale:
// - Decouples the external HTTP contract from Application-layer types and allows Presentation-layer enrichment.
//
// Future Extension Seams:
// - Add CallerUserId or resolved permission context here when commands require Presentation-layer enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.DataLake.Models.Requests;

/// <summary>Internal Presentation-layer command request for DataLake create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="DatasetName">Name of the dataset associated with this record.</param>
/// <param name="Zone">Data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</param>
/// <param name="RecordCount">Number of records processed in this activity.</param>
/// <param name="ProcessedBy">Identity of the user or process that performed the activity.</param>
public sealed record DataLakeCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   DatasetName,
    string   Zone,
    long?   RecordCount,
    Guid?    ProcessedBy);