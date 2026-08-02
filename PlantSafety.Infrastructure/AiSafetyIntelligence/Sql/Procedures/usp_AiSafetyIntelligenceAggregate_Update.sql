-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AiSafetyIntelligence
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AiSafetyIntelligenceAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of an AiSafetyIntelligence aggregate root
--                  row. Backs IAiSafetyIntelligenceRepository.UpdateAsync. @TenantId is part of the
--                  WHERE clause and @ETag enforces optimistic concurrency against the native
--                  RowVersion column from day one. @RowsAffected is captured via @@ROWCOUNT
--                  immediately after the UPDATE (before the audit INSERT) and returned via SELECT
--                  -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_Update]
    @AiSafetyIntelligenceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @InsightType NVARCHAR(50),
    @Description NVARCHAR(2000),
    @ConfidenceScore DECIMAL(5,4),
    @SourceModelName NVARCHAR(200),
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate] WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'AiSafetyIntelligence not found or has been deleted.', 1;

        UPDATE [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate]
        SET
            Status = @Status,
            InsightType = @InsightType,
            Description = @Description,
            ConfidenceScore = @ConfidenceScore,
            SourceModelName = @SourceModelName,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AiSafetyIntelligenceAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; User=', SYSTEM_USER);
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
