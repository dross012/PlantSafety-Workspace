// ============================================================================
// File: CreateBudgetResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned after successfully creating
//              a Safety Budget.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.Budgets.Contracts.Responses;

/// <summary>Response payload returned after creating a Safety Budget.</summary>
/// <param name="BudgetId">Identifier of the newly created budget.</param>
public sealed record CreateBudgetResponse(Guid BudgetId);
