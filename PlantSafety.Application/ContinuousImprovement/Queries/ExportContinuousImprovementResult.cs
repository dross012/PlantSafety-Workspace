// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Result payload for ExportContinuousImprovementQuery.
// SchemaVersion: 1

namespace PlantSafety.Application.ContinuousImprovement.Queries;

/// <summary>Result of <see cref="ExportContinuousImprovementQuery"/>: a presigned download URL and file metadata.</summary>
/// <param name="FileName">The suggested download file name.</param>
/// <param name="ContentType">MIME type of the generated file.</param>
/// <param name="DownloadUrl">Presigned Azure Blob SAS URL. Expires at <paramref name="ExpiresAt"/>.</param>
/// <param name="ExpiresAt">UTC timestamp when the download URL becomes invalid.</param>
/// <param name="RecordCount">Actual number of records included in the export.</param>
public sealed record ExportContinuousImprovementResult(
    string FileName,
    string ContentType,
    string DownloadUrl,
    DateTimeOffset ExpiresAt,
    int RecordCount);
