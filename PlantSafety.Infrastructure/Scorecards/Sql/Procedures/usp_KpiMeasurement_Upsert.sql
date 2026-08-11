-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_KpiMeasurement_Upsert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new KpiMeasurement row, or updates all mutable fields of an existing
--                  one. Matched by (ScorecardId, Metric) rather than KpiMeasurementId, because
--                  Scorecard.RecordMetric() always removes any prior measurement for the same
--                  metric code and creates a brand-new KpiMeasurement (with a new Id) in its
--                  place -- matching by the natural (ScorecardId, Metric) key correctly overwrites
--                  the prior row instead of accumulating stale duplicates. Used by
--                  IScorecardRepository.UpdateAsync to reconcile the in-memory
--                  Scorecard.Measurements collection. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scorecards].[usp_KpiMeasurement_Upsert]
    @KpiMeasurementId UNIQUEIDENTIFIER,
    @ScorecardId      UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @Metric           NVARCHAR(50),
    @Numerator        DECIMAL(18,4),
    @Denominator      DECIMAL(18,4),
    @ComputedValue    DECIMAL(18,4),
    @DataSourceModule NVARCHAR(200),
    @RecordedAtUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM [Scorecards].[KpiMeasurement] WHERE ScorecardId = @ScorecardId AND Metric = @Metric AND IsDeleted = 0)
        BEGIN
            UPDATE [Scorecards].[KpiMeasurement]
            SET KpiMeasurementId = @KpiMeasurementId, Numerator = @Numerator, Denominator = @Denominator,
                ComputedValue = @ComputedValue, DataSourceModule = @DataSourceModule, RecordedAtUtc = @RecordedAtUtc
            WHERE ScorecardId = @ScorecardId AND Metric = @Metric AND IsDeleted = 0;
        END
        ELSE
        BEGIN
            INSERT INTO [Scorecards].[KpiMeasurement]
                (KpiMeasurementId, ScorecardId, TenantId, Metric, Numerator, Denominator, ComputedValue, DataSourceModule, RecordedAtUtc, IsDeleted)
            VALUES
                (@KpiMeasurementId, @ScorecardId, @TenantId, @Metric, @Numerator, @Denominator, @ComputedValue, @DataSourceModule, @RecordedAtUtc, 0);
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'KpiMeasurement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'KpiMeasurementId=', @KpiMeasurementId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'KpiMeasurementId=', @KpiMeasurementId, N'; ScorecardId=', @ScorecardId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
