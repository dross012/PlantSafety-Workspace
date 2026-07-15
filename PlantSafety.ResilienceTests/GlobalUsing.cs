// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ResilienceTests
//  Layer:          Tests (Resilience / Chaos / Failover / Recovery)
//  Description:    Global usings for the Resilience test suite covering the
//                  four formally distinct 17-type-manual test types RESL,
//                  CHAO, FAIL and RCVR (tests_rules_design_manual.md),
//                  consolidated into one project per the task's stated
//                  granularity guidance ("Failover/Recovery could share a
//                  project"). All tests are deterministic (fixed seeds,
//                  synthetic timestamps, no real network/DB dependency) per
//                  the manual's Foundational Principles.
// ============================================================================
global using System.Text.Json;
global using FluentAssertions;
global using Microsoft.Extensions.Logging.Abstractions;
global using Xunit;
global using PlantSafety.Domain.Common.Abstractions;
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Operational.Alerts.Events;
global using PlantSafety.Domain.Common.Types.StrongIds;
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;
global using PlantSafety.Infrastructure.Persistence.UnitOfWork;
