// ============================================================================
// File: CreateDocumentsRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new synced document record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateDocumentsCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add file content-hash or storage URI metadata here when Documents records support direct blob linkage.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Documents.Contracts.Requests;

/// <summary>Request body for creating a new synced document record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="SourceSystem">External document management system the document was synced from.</param>
/// <param name="FileName">Name of the synced document file.</param>
/// <param name="DocumentType">Classification of the synced document (e.g., PDF/Policy/Certificate).</param>
public sealed record CreateDocumentsRequest(
    Guid   TenantId,
    Guid   SiteId,
    string SourceSystem,
    string FileName,
    string DocumentType);
