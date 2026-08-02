// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CertificationStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Certification lifecycle-transition commands
//                  (Verify, Renew, Expire, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Certification.Commands;

/// <summary>Result payload shared by Certification lifecycle-transition commands.</summary>
public sealed record CertificationStatusResult(Guid CertificationId, string Status);
