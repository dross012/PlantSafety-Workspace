// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Abstraction for durably storing and retrieving the rendered byte-stream
//   content of a generated AuditReport (PDF/CSV/JSON). Kept separate from
//   IAuditReportRepository (which persists the aggregate's structured state) because rendered
//   artifacts are typically large binary blobs best stored in blob storage rather than a
//   relational read-model row.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditReport.Enums;

namespace PlantSafety.Application.Auditing.AuditReport.Repositories;

/// <summary>Durably stores and retrieves rendered AuditReport byte-stream content.</summary>
public interface IAuditReportContentStore
{
    /// <summary>Saves rendered content and returns an opaque storage reference for later retrieval.</summary>
    Task<string> SaveAsync(Guid tenantId, Guid auditReportId, AuditReportFormat format, byte[] content, CancellationToken cancellationToken);

    /// <summary>Retrieves previously saved rendered content by its storage reference.</summary>
    Task<byte[]?> GetAsync(string contentReference, CancellationToken cancellationToken);
}
