// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side DTOs for the FinancialRecord aggregate (the
//   "costs" input surface — a distinct aggregate from Budget). Mirrors
//   PlantSafety.Api.Financials.Contracts.Requests.CreateFinancialRecordRequest
//   and PlantSafety.Application.Financials.Commands.CreateFinancialRecordResult.
//   Backed by POST api/v1/financial-records (FinancialRecordV1Controller),
//   NOT api/v1/features/financials/... — see FinancialsApiClient remarks.
//   Scoped to the data-entry (Create) flow only; no list/detail surface is
//   built for this aggregate yet (no GET-list endpoint exists — only
//   GetById), per the UI Agent's build-what-the-backend-supports mandate.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Financials.Models;

/// <summary>
/// Request body for creating a new FinancialRecord (Status=Draft). Mirrors
/// PlantSafety.Api.Financials.Contracts.Requests.CreateFinancialRecordRequest.
/// Backed by POST v1/financial-records.
/// </summary>
public sealed class CreateFinancialRecordRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Financial category (e.g. Budget, Cost, Forecast).</summary>
    [Required(ErrorMessage = "Category is required.")]
    [StringLength(200, ErrorMessage = "Category must be 200 characters or fewer.")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Monetary amount. Must be positive.</summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}

/// <summary>
/// Result payload returned after creating a new FinancialRecord. Mirrors
/// PlantSafety.Application.Financials.Commands.CreateFinancialRecordResult.
/// </summary>
public sealed class CreateFinancialRecordResult
{
    /// <summary>Gets the identifier of the newly created financial record.</summary>
    public Guid FinancialRecordId { get; init; }

    /// <summary>Gets the record's lifecycle status immediately after creation (Draft).</summary>
    public string Status { get; init; } = string.Empty;
}
