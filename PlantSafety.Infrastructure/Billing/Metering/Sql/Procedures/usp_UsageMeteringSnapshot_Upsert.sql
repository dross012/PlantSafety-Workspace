-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.Metering
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_UsageMeteringSnapshot_Upsert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
--  SchemaVersion:  2026.1
--  Description:    Upserts a UsageMeteringSnapshot row (re-capturing the same tenant/period
--                  replaces the prior snapshot). Backs IUsageMeteringSnapshotRepository.SaveAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_UsageMeteringSnapshot_Upsert]
    @UsageMeteringSnapshotId UNIQUEIDENTIFIER,
    @TenantId                UNIQUEIDENTIFIER,
    @PeriodYear              INT,
    @PeriodMonth             INT,
    @CapturedAtUtc           DATETIME2(3),
    @RecordedValuesJson      NVARCHAR(MAX)
AS
SET NOCOUNT ON;
BEGIN
    MERGE [Billing].[UsageMeteringSnapshot] AS target
    USING (SELECT @TenantId AS TenantId, @PeriodYear AS PeriodYear, @PeriodMonth AS PeriodMonth) AS source
        ON target.TenantId = source.TenantId AND target.PeriodYear = source.PeriodYear AND target.PeriodMonth = source.PeriodMonth
    WHEN MATCHED THEN
        UPDATE SET
            UsageMeteringSnapshotId = @UsageMeteringSnapshotId,
            CapturedAtUtc = @CapturedAtUtc,
            RecordedValuesJson = @RecordedValuesJson
    WHEN NOT MATCHED THEN
        INSERT (UsageMeteringSnapshotId, TenantId, PeriodYear, PeriodMonth, CapturedAtUtc, RecordedValuesJson)
        VALUES (@UsageMeteringSnapshotId, @TenantId, @PeriodYear, @PeriodMonth, @CapturedAtUtc, @RecordedValuesJson);
END
GO
