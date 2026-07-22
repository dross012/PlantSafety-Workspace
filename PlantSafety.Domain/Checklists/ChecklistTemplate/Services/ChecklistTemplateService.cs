// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for ChecklistTemplate. Coordinates logic that cannot live
//                  on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Services;

/// <summary>Domain service for ChecklistTemplate. Coordinates logic that cannot live on a single aggregate.</summary>
public class ChecklistTemplateService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}