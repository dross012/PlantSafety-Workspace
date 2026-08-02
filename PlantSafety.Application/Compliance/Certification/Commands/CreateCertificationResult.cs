// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CreateCertificationResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateCertificationCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Certification.Commands;

/// <summary>Result of a successful <see cref="CreateCertificationCommand"/> execution.</summary>
public sealed record CreateCertificationResult(
    Guid CertificationId,
    string Status);
