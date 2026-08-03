// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve a single Integrations.Documents read-model record by id.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Documents.Models;

namespace PlantSafety.Application.Integrations.Documents.Queries;

/// <summary>
/// Retrieves a single Integrations.Documents record by its identifier, scoped to the caller's
/// tenant. Returns a successful <see cref="Result{T}"/> wrapping <see langword="null"/> when no
/// matching record exists (not-found is not a failure condition here — the controller maps it to
/// HTTP 404).
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="DocumentId">The synced document record identifier to retrieve.</param>
public sealed record GetDocumentByIdQuery(
    Guid TenantId,
    Guid DocumentId) : IRequest<Result<DocumentsItemDto?>>;
