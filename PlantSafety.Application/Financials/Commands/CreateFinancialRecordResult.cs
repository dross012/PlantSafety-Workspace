// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           CreateFinancialRecordResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateFinancialRecordCommand. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Financials.Commands;

/// <summary>Result payload returned after creating a new FinancialRecord.</summary>
public sealed record CreateFinancialRecordResult(Guid FinancialRecordId, string Status);
