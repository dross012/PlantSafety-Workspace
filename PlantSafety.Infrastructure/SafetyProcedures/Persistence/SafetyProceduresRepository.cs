// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures
//  Layer:          Infrastructure
//  File:           SafetyProceduresRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  ISafetyProceduresRepository (declared with zero members in the Domain
//                  layer; the real persistence surface for this module lives entirely in the
//                  8 sub-component repositories under SafetyProcedures/*/Persistence).
//                  Provided purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.Repositories;

namespace PlantSafety.Infrastructure.SafetyProcedures.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="ISafetyProceduresRepository"/> marker.</summary>
public sealed class SafetyProceduresRepository : ISafetyProceduresRepository;
