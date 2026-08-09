// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Training
//  File:           EmployeeCertification.cs
//  Created:        2026-08-08
//  Description:    Phase C.1: a single issued certification held by an
//                  employee. EmployeeState holds the current list of these
//                  directly (no separate "profile" wrapper -- the list
//                  itself, replaced wholesale on update, is the profile).
// ============================================================================
namespace Shared.EmployeeDomain.Abstractions.Training;

/// <summary>A single certification issued to an employee.</summary>
public sealed record EmployeeCertification(
    string CertificationCode,
    string IssuingBody,
    DateTime IssuedAtUtc,
    DateTime? ExpiresAtUtc)
{
    /// <summary>True if this certification has an expiration date that has already passed, relative to <paramref name="nowUtc"/>.</summary>
    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc is not null && ExpiresAtUtc.Value <= nowUtc;
}
