// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SmokeTests
//  Layer:          Tests (Smoke)
//  Description:    Global usings for the Smoke test suite (PLNT-SMKE-*).
//                  Smoke tests verify the built system starts and the newly
//                  built Compliance/Analytics/CrossDomain/SafetyEvents
//                  repository layer resolves cleanly from DI, per
//                  tests_rules_design_manual.md (SMKE test type).
// ============================================================================
global using System.Reflection;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using NSubstitute;
global using FluentAssertions;
global using Xunit;
global using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
global using PlantSafety.Infrastructure.Persistence.Dapper.Commands;
global using PlantSafety.Infrastructure.Persistence.Dapper.Queries;
global using PlantSafety.Infrastructure.Persistence.Repositories.Compliance;
global using PlantSafety.Infrastructure.Persistence.Repositories.Analytics;
global using PlantSafety.Infrastructure.Persistence.Repositories.CrossDomain;
global using PlantSafety.Infrastructure.Persistence.Repositories.SafetyEvents;
