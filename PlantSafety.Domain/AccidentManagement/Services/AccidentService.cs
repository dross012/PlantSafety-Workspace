// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for Accident. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;

namespace PlantSafety.Domain.AccidentManagement.Services;

/// <summary>Domain service for Accident. Coordinates logic that cannot live on a single aggregate.</summary>
public class AccidentService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}