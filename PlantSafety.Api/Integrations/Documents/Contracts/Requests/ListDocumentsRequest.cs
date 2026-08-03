// ============================================================================
// File: ListDocumentsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: External-facing DTO for GET /integrations/documents/list query parameters,
//   bound from query string by the V1 controller.
// ============================================================================
//
// Onboarding Notes:
// - Validated by FluentValidation via ApiValidationFilter before MediatR dispatch.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer query types.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Documents.Contracts.Requests;

/// <summary>Query parameters for retrieving a paginated, filterable list of Integrations.Documents data.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="Category">Optional document-type ("category") filter (e.g. Policy, Procedure, Training, Compliance).</param>
/// <param name="Search">Optional free-text search matched against the synced document's file name.</param>
/// <param name="PageNumber">1-based page number. Defaults to 1.</param>
/// <param name="PageSize">Items per page. Defaults to 25, capped at the platform maximum.</param>
public sealed record ListDocumentsRequest(
    Guid      TenantId,
    Guid?     SiteId,
    string?   Category,
    string?   Search,
    int       PageNumber = 1,
    int       PageSize = 25);
