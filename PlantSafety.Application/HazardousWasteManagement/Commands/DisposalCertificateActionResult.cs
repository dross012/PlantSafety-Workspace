// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for DisposalCertificate commands ("HazardousWasteManagement.DisposalCertificates").
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Result of a successful DisposalCertificate command execution.</summary>
/// <param name="DisposalCertificateId">Identity of the affected DisposalCertificate aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record DisposalCertificateActionResult(
    Guid DisposalCertificateId,
    string Status);
