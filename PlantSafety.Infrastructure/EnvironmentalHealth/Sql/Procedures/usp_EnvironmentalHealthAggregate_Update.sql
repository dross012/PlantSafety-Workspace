-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnvironmentalHealthAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of an EnvironmentalHealth aggregate root
--                  row. Backs IEnvironmentalHealthRepository.UpdateAsync. @TenantId is part of
--                  the WHERE clause and @ETag enforces optimistic concurrency against the
--                  native RowVersion column from day one, mirroring
--                  usp_AuditingAggregate_Update.sql. @RowsAffected is captured via @@ROWCOUNT
--                  immediately after the UPDATE (before the audit INSERT) and returned via
--                  SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery
--                  always return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EnvironmentalHealthAggregate_Update]
    @EnvironmentalHealthId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @MonitoringType NVARCHAR(100),
    @MeasuredValue DECIMAL(18,4) = NULL,
    @Unit NVARCHAR(50),
    @Threshold DECIMAL(18,4),
    @ThresholdExceeded BIT,
    @EvaluatedStatus NVARCHAR(50),
    @RecordedAtUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [EnvironmentalHealth].[EnvironmentalHealthAggregate] WHERE EnvironmentalHealthId = @EnvironmentalHealthId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'EnvironmentalHealth record not found or has been deleted.', 1;

        UPDATE [EnvironmentalHealth].[EnvironmentalHealthAggregate]
        SET
            Status = @Status,
            MonitoringType = @MonitoringType,
            MeasuredValue = @MeasuredValue,
            Unit = @Unit,
            Threshold = @Threshold,
            ThresholdExceeded = @ThresholdExceeded,
            EvaluatedStatus = @EvaluatedStatus,
            RecordedAtUtc = @RecordedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE EnvironmentalHealthId = @EnvironmentalHealthId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EnvironmentalHealthAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
