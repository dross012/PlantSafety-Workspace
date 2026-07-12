// ============================================================================
// File: DocumentsCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Integrations.Documents queries and commands.
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

namespace PlantSafety.Api.Integrations.Documents.Models.Requests;

/// <summary>Internal Presentation-layer command request for Integrations.Documents create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="SourceSystem">External document management system the document was synced from.</param>
/// <param name="FileName">Name of the synced document file.</param>
/// <param name="DocumentType">Classification of the synced document (e.g., PDF/Policy/Certificate).</param>
public sealed record DocumentsCommandRequest(
    Guid   TenantId,
    Guid   SiteId,
    string SourceSystem,
    string FileName,
    string DocumentType);
