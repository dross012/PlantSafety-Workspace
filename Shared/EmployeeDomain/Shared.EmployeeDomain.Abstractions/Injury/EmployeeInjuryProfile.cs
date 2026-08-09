// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Injury
//  File:           EmployeeInjuryProfile.cs
//  Created:        2026-08-08
//  Description:    Phase C.1: an employee's cumulative injury history
//                  summary. Kept self-contained (no dependency on
//                  Phase C.3's SeverityLevel, which does not exist yet at
//                  this point in the build) -- severity-aware injury
//                  scoring is layered on top in Phase C.3/C.10, not here.
// ============================================================================
namespace Shared.EmployeeDomain.Abstractions.Injury;

/// <summary>An employee's cumulative injury history, as of <see cref="UpdatedAtUtc"/>.</summary>
public sealed record EmployeeInjuryProfile(
    int TotalInjuryCount,
    DateTime? LastInjuryAtUtc,
    string? LastInjuryDescription,
    DateTime UpdatedAtUtc)
{
    /// <summary>An empty injury profile, for a newly created employee.</summary>
    public static EmployeeInjuryProfile Empty(DateTime nowUtc) => new(0, null, null, nowUtc);

    /// <summary>Returns a new profile with one additional recorded injury.</summary>
    public EmployeeInjuryProfile RecordInjury(string description, DateTime nowUtc) =>
        this with { TotalInjuryCount = TotalInjuryCount + 1, LastInjuryAtUtc = nowUtc, LastInjuryDescription = description, UpdatedAtUtc = nowUtc };
}
