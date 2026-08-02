// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           CreateComplianceResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateComplianceCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Compliance.Commands;

/// <summary>Result of a successful <see cref="CreateComplianceCommand"/> execution.</summary>
public sealed record CreateComplianceResult(
    Guid ComplianceId,
    string Status);
