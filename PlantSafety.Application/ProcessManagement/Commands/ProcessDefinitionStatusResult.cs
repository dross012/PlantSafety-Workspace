// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           ProcessDefinitionStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the ProcessDefinition lifecycle-transition commands
//                  (SubmitForReview, Approve, Retire, Archive, Revise). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Commands;

/// <summary>Result payload shared by ProcessDefinition lifecycle-transition commands.</summary>
public sealed record ProcessDefinitionStatusResult(Guid ProcessDefinitionId, string Status, int Version);
