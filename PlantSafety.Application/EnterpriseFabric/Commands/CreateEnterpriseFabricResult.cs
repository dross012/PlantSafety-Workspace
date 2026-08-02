// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           CreateEnterpriseFabricResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateEnterpriseFabricCommand. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.EnterpriseFabric.Commands;

/// <summary>Result payload returned after creating a new EnterpriseFabric record.</summary>
public sealed record CreateEnterpriseFabricResult(Guid EnterpriseFabricId, string Status);
