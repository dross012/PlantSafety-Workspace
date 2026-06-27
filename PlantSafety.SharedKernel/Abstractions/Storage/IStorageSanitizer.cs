/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageSanitizer.cs
    DESCRIPTION:        SIEM-safe sanitization abstraction for storage identifiers, file names, and metadata fields.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Sanitizes storage identifiers and metadata for SIEM-safe logging.
/// </summary>
public interface IStorageSanitizer
{
    string Sanitize(string value);
}
