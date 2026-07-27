// ============================================================================
// File: GlobalUsing.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Global using directives for the PlantSafety.Api Presentation layer.
//              Reduces per-file boilerplate while keeping the import surface explicit.
// ============================================================================
//
// Onboarding Notes:
// - Only namespaces that are genuinely used across the majority of files in this project
//   belong here. Module-specific namespaces (e.g., PlantSafety.Api.Analytics.Dashboard)
//   should be declared per-file, not globally.
// - The Infrastructure layer namespaces (Serilog, OpenTelemetry, StackExchange.Redis,
//   Dapper, MassTransit) must NOT appear here. The Presentation layer references
//   Infrastructure only via the DI registration extensions.
// - MediatR is globally imported because all controllers dispatch commands/queries via ISender.
//
// Rationale:
// - Prevents repetitive boilerplate across hundreds of controller files while maintaining
//   Clean Architecture layer discipline in the import surface.
//
// Future Extension Seams:
// - As new cross-cutting concerns are added (e.g., rate limiting, feature flags), their
//   primary ASP.NET Core abstractions may be promoted to global usings here.
//
// ============================================================================

// ── BCL ───────────────────────────────────────────────────────────────────────
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// ── ASP.NET Core ─────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

// ── MediatR ──────────────────────────────────────────────────────────────────
global using MediatR;

// ── API Versioning ────────────────────────────────────────────────────────────
global using Asp.Versioning;

// ── PlantSafety Presentation layer ───────────────────────────────────────────
global using PlantSafety.Api.Models;
global using PlantSafety.Api.Middleware;

// ── PlantSafety Shared Kernel ─────────────────────────────────────────────────
// Result pattern, StrongId — core cross-cutting types
global using PlantSafety.SharedKernel;
global using PlantSafety.SharedKernel.Primitives.Results;

// ── Shared Authorization ────────────────────────────────────────────────────
// Canonical Safety-domain role and policy-name constants (see
// docs/architecture/authorization-tiers.md). Prevents role-name string drift
// across controllers and across the PlantSafety/Reporting module boundary.
global using Shared.Authorization.Abstractions.Roles;
global using Shared.Authorization.Abstractions.Policies;
