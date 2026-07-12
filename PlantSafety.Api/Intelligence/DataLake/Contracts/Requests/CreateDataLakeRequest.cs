// ============================================================================
// File: CreateDataLakeRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new DataLake record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateDataLakeCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add additional metadata fields here as the DataLake record schema evolves.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.DataLake.Contracts.Requests;

/// <summary>Request body for creating a new DataLake record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="DatasetName">Name of the dataset associated with this record.</param>
/// <param name="Zone">Data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</param>
/// <param name="RecordCount">Number of records processed in this activity.</param>
/// <param name="ProcessedBy">Identity of the user or process that performed the activity.</param>
public sealed record CreateDataLakeRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   DatasetName,
    string   Zone,
    long?   RecordCount,
    Guid?    ProcessedBy);