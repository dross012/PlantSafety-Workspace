-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.Metering
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_UsageMeteringSnapshot_GetByTenantPeriod.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the snapshot for a given tenant/period. Backs
--                  IUsageMeteringSnapshotRepository.GetAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_UsageMeteringSnapshot_GetByTenantPeriod]
    @TenantId    UNIQUEIDENTIFIER,
    @PeriodYear  INT,
    @PeriodMonth INT
AS
SET NOCOUNT ON;
BEGIN
    SELECT UsageMeteringSnapshotId, TenantId, PeriodYear, PeriodMonth, CapturedAtUtc, RecordedValuesJson
    FROM [Billing].[UsageMeteringSnapshot]
    WHERE TenantId = @TenantId AND PeriodYear = @PeriodYear AND PeriodMonth = @PeriodMonth;
END
GO
