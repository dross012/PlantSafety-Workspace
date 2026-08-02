// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           FinancialRecordByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single FinancialRecord read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Named "FinancialRecordByIdQuery" (NOT
//                  "GetFinancialsQuery", which already exists and is unrelated -- a date/list-style
//                  read query left entirely untouched). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Queries.GetRCAByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Financials.Models;

namespace PlantSafety.Application.Financials.Queries;

/// <summary>Retrieves a single FinancialRecord read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="FinancialRecordId">Identity of the FinancialRecord to retrieve.</param>
public sealed record FinancialRecordByIdQuery(
    Guid TenantId,
    Guid FinancialRecordId) : IRequest<Result<FinancialRecordDetailDto>>;
