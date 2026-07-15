// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DataIntegrationTests
//  Layer:          Tests (Data Integration)
//  Description:    Global usings for the Data Integration test suite
//                  (PLNT-DINT-*), covering the DINT type from
//                  tests_rules_design_manual.md. Validates the checked-in
//                  canonical stored-procedure SQL scripts against
//                  project_plantsafety_patterns.md's mandatory header
//                  parameter contract (@TenantId/@CorrelationId/@ActorId/
//                  @NowUtc), the Dapper parameter-builder contract, and the
//                  Uuid7 time-ordered identifier contract that underpins
//                  safe cross-system data-integration replay ordering.
//                  No live SQL Server is required -- these tests operate on
//                  the .sql files checked into the repository and on the
//                  pure identifier-generation code.
// ============================================================================
global using System.IO;
global using Dapper;
global using FluentAssertions;
global using Xunit;
global using PlantSafety.Infrastructure.Persistence.Dapper.SqlBuilder;
global using PlantSafety.SharedKernel.Identifiers;
