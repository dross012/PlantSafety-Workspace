// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training.CompetencyManagement
//  Layer:          Infrastructure
//  File:           ICompetencyManagementInfrastructure.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Infrastructure contract for the CompetencyManagement module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Training.CompetencyManagement.Interfaces;

/// <summary>Infrastructure service contract for the <c>CompetencyManagement</c> module.</summary>
public interface ICompetencyManagementInfrastructure
{
    Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
