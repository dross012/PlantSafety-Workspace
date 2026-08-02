// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           ContractorVisitorStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the ContractorVisitor lifecycle-transition commands
//                  (Activate, CheckIn, Complete, Close, Archive). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ContractorVisitor.Commands;

/// <summary>Result payload shared by ContractorVisitor lifecycle-transition commands.</summary>
public sealed record ContractorVisitorStatusResult(Guid ContractorVisitorId, string Status);
