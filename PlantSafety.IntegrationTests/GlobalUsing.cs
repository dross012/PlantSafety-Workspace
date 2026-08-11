// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests
//  Layer:          IntegrationTests
//  File:           GlobalUsing.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Project-wide global usings for all integration test files.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
// ---- System ----------------------------------------------------------------
global using System.Data;
global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;

// ---- xunit -----------------------------------------------------------------
global using Xunit;

// ---- FluentAssertions -------------------------------------------------------
global using FluentAssertions;

// ---- Dapper ----------------------------------------------------------------
global using Dapper;

// ---- Microsoft -------------------------------------------------------------
global using Microsoft.Data.SqlClient;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Mvc.Testing;

// ---- Testcontainers --------------------------------------------------------
global using Testcontainers.MsSql;

// ---- PlantSafety -----------------------------------------------------------
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;
global using PlantSafety.Domain.Common.Abstractions;
global using PlantSafety.Domain.Common.Types.StrongIds;

// ---- IntegrationTests: Setup -----------------------------------------------
global using PlantSafety.IntegrationTests.Setup;
global using PlantSafety.IntegrationTests.Common.Assertions;
global using PlantSafety.IntegrationTests.Common.TestConstants;

// TenantHttpContextTestHelper: tests that send a request through the real MediatR pipeline must
// establish a server-resolved tenant context first, or TenantIsolationBehavior sees no
// authenticated tenant and rejects the request as a cross-tenant access attempt.
global using PlantSafety.IntegrationTests.Tenancy;
