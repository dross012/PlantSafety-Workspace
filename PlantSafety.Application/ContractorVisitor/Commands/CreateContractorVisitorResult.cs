// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           CreateContractorVisitorResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateContractorVisitorCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ContractorVisitor.Commands;

/// <summary>Result of a successful <see cref="CreateContractorVisitorCommand"/> execution.</summary>
public sealed record CreateContractorVisitorResult(
    Guid ContractorVisitorId,
    string Status);
