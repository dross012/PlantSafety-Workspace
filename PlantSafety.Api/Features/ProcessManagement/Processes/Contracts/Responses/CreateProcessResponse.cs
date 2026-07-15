// ============================================================================
// File: CreateProcessResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTO returned after successfully creating
//              a Process Definition.
// ============================================================================

namespace PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Responses;

/// <summary>Response payload returned after creating a Process Definition.</summary>
/// <param name="ProcessId">Identifier of the newly created process.</param>
public sealed record CreateProcessResponse(Guid ProcessId);
