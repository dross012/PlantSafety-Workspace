// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety
//  Layer:          Infrastructure
//  File:           CommunitySafetyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  ICommunitySafetyRepository (declared with zero members in the Domain layer;
//                  the real persistence surface for this module lives entirely in the 5
//                  sub-component repositories under CommunitySafety/*/Persistence). Provided
//                  purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.Repositories;

namespace PlantSafety.Infrastructure.CommunitySafety.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="ICommunitySafetyRepository"/> marker.</summary>
public sealed class CommunitySafetyRepository : ICommunitySafetyRepository;
