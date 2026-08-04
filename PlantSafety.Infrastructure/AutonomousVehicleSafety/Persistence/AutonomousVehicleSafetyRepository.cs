// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety
//  Layer:          Infrastructure
//  File:           AutonomousVehicleSafetyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  IAutonomousVehicleSafetyRepository (declared with zero members in the Domain
//                  layer; the real persistence surface for this module lives entirely in the 5
//                  sub-component repositories under AutonomousVehicleSafety/*/Persistence).
//                  Provided purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.Repositories;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="IAutonomousVehicleSafetyRepository"/> marker.</summary>
public sealed class AutonomousVehicleSafetyRepository : IAutonomousVehicleSafetyRepository;
