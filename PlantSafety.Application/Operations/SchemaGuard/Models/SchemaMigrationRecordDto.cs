// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SchemaGuard
//  Layer:          Application
//  File:           SchemaMigrationRecordDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    A single row from a database's [System].[SchemaMigrations] table
//                  (Phase 7 Governance, 70-schema-migrations-tracking.sql). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.SchemaGuard.Models;

/// <summary>Projection of a single applied-migration row from [System].[SchemaMigrations].</summary>
public sealed record SchemaMigrationRecordDto(
    string ScriptName,
    string? Phase,
    DateTime AppliedAtUtc,
    string AppliedBy,
    bool Success,
    string? Notes);
