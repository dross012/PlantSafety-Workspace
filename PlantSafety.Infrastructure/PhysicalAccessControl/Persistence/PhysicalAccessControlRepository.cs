// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl
//  Layer:          Infrastructure
//  File:           PhysicalAccessControlRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  IPhysicalAccessControlRepository (declared with zero members in the Domain
//                  layer; the real persistence surface for this module lives entirely in the 6
//                  entity-scoped repositories under PhysicalAccessControl/*/Persistence).
//                  Provided purely so the marker interface has a concrete, DI-registerable
//                  implementation, mirroring the MachineSafety top-level marker pattern.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.Repositories;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="IPhysicalAccessControlRepository"/> marker.</summary>
public sealed class PhysicalAccessControlRepository : IPhysicalAccessControlRepository;
