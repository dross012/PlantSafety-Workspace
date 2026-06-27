/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DataLakeDomainService.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.DataLake.Entities;
using Intelligence.Domain.DataLake.ValueObjects;

namespace Intelligence.Domain.DataLake.Services;

/// <summary>Stateless domain service for Data Lake business rules. No I/O permitted.</summary>
public sealed class DataLakeDomainService
{
    /// <summary>Returns true when a candidate record duplicates an existing record (same source + externalRecordId).</summary>
    public bool IsDuplicate(DataLakeRecord candidate, IEnumerable<DataLakeRecord> existing)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(existing);
        return existing.Any(r =>
            r.Source.Equals(candidate.Source) &&
            string.Equals(r.ExternalRecordId, candidate.ExternalRecordId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Returns true when two schema versions share the same year prefix, making them compatible for transformation.</summary>
    public bool AreCompatibleForTransformation(SchemaVersion from, SchemaVersion to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        return string.Equals(from.Value.Split('.')[0], to.Value.Split('.')[0], StringComparison.Ordinal);
    }
}
