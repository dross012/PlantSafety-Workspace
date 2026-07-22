// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain service for PermitWorkflow. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Services;

/// <summary>Domain service for PermitWorkflow. Coordinates logic that cannot live on a single aggregate.</summary>
public class PermitWorkflowService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}