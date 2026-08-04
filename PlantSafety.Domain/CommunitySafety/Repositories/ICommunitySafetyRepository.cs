// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety
//  Layer:          Domain
//  File:           ICommunitySafetyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Placeholder repository interface for top-level CommunitySafety, mirroring the
//                  convention used by other maximalist modules (e.g. MachineSafety, SafetyEvents)
//                  where the module-level repository is a no-op marker and all real persistence
//                  is owned by each sub-component's own repository interface. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.Repositories;

/// <summary>Placeholder repository for top-level CommunitySafety.</summary>
public interface ICommunitySafetyRepository { }
