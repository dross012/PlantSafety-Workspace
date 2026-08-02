// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           FinancialRecordStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the FinancialRecord lifecycle-transition commands
//                  (AddLineItem, Submit, Approve, Reject, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Financials.Commands;

/// <summary>Result payload shared by FinancialRecord lifecycle-transition commands.</summary>
public sealed record FinancialRecordStatusResult(Guid FinancialRecordId, string Status);
