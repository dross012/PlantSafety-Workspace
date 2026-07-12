// ============================================================================
// File: UpdateDocumentsRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing synced document record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateDocumentsCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Documents records.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Documents.Contracts.Requests;

/// <summary>Request body for updating an existing synced document record.</summary>
/// <param name="FileName">Updated file name.</param>
/// <param name="Status">Updated sync status value.</param>
public sealed record UpdateDocumentsRequest(
    string FileName,
    string Status);
