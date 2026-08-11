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

BEGIN TRY
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

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'UsageMeteringSnapshot', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'UsageMeteringSnapshotId=', @UsageMeteringSnapshotId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'UsageMeteringSnapshotId=', @UsageMeteringSnapshotId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
END CATCH
GO
