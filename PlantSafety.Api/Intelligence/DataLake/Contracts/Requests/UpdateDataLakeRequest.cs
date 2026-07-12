// ============================================================================
// File: UpdateDataLakeRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing DataLake record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateDataLakeCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for DataLake.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.DataLake.Contracts.Requests;

/// <summary>Request body for updating an existing DataLake record.</summary>
/// <param name="DatasetName">Updated name of the dataset associated with this record.</param>
/// <param name="Zone">Updated data lake zone the record resides in (e.g., raw/cleansed/curated/ml/sandbox).</param>
/// <param name="RecordCount">Updated number of records processed in this activity.</param>
/// <param name="ProcessedBy">Updated identity of the user or process that performed the activity.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateDataLakeRequest(
    string   DatasetName,
    string   Zone,
    long?   RecordCount,
    Guid?    ProcessedBy,
    string   Status);