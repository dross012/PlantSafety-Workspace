// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SecurityTests
//  Layer:          Tests (Security / Penetration / Auth-Identity)
//  Description:    Global usings for the Security test suite covering the
//                  three formally distinct 17-type-manual test types SEC,
//                  PEN and AUTH (tests_rules_design_manual.md), consolidated
//                  into one project per the task's stated granularity
//                  guidance. Covers authz boundaries, tenant isolation, and
//                  SQL-injection resistance (Dapper parameterization).
// ============================================================================
global using System.Security.Claims;
global using Dapper;
global using FluentAssertions;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Logging.Abstractions;
global using NSubstitute;
global using Xunit;
global using PlantSafety.Application.Abstractions.CurrentUser;
global using PlantSafety.Application.Abstractions.Security;
global using PlantSafety.Infrastructure.Identity;
global using PlantSafety.Infrastructure.Persistence.Dapper.SqlBuilder;
