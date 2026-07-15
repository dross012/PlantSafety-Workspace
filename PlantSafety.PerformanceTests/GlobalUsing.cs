// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PerformanceTests
//  Layer:          Tests (Performance)
//  Description:    Global usings for the Performance test suite (PLNT-PERF-*).
//                  Enforces the hard numeric thresholds from
//                  tests_rules_design_manual.md Ch.9 (PlantSafety-specific
//                  hard numbers) and the principles of
//                  performance_stress_manual.md (async-first, deterministic,
//                  no Thread.Sleep, no blocking I/O in the measured path).
// ============================================================================
global using System.Diagnostics;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using NSubstitute;
global using FluentAssertions;
global using Xunit;
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Common.Types.StrongIds;
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;
