// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety
//  Layer:          Infrastructure
//  File:           MachineSafetyRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  IMachineSafetyRepository (declared with zero members in the Domain layer;
//                  the real persistence surface for this module lives entirely in the 6
//                  sub-component repositories under MachineSafety/*/Persistence). Provided
//                  purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.Repositories;

namespace PlantSafety.Infrastructure.MachineSafety.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="IMachineSafetyRepository"/> marker.</summary>
public sealed class MachineSafetyRepository : IMachineSafetyRepository;
