-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AuditingAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of an Auditing aggregate root row. Backs
--                  IAuditingRepository.UpdateAsync. @TenantId is part of the WHERE clause and
--                  @ETag enforces optimistic concurrency against the native RowVersion column
--                  from day one, mirroring usp_Permits_Update.sql. @RowsAffected is captured via
--                  @@ROWCOUNT immediately after the UPDATE (before the audit INSERT) and returned
--                  via SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery
--                  always return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_AuditingAggregate_Update]
    @AuditingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @AuditTitle NVARCHAR(400),
    @AuditType NVARCHAR(100),
    @Scope NVARCHAR(2000),
    @AuditorId UNIQUEIDENTIFIER,
    @ScheduledAtUtc DATETIME2(3) = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @FindingsCount INT,
    @Score DECIMAL(5,2) = NULL,
    @ComplianceRating NVARCHAR(50),
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Compliance].[AuditingAggregate] WHERE AuditingId = @AuditingId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'Auditing not found or has been deleted.', 1;

        UPDATE [Compliance].[AuditingAggregate]
        SET
            Status = @Status,
            AuditTitle = @AuditTitle,
            AuditType = @AuditType,
            Scope = @Scope,
            AuditorId = @AuditorId,
            ScheduledAtUtc = @ScheduledAtUtc,
            CompletedAtUtc = @CompletedAtUtc,
            FindingsCount = @FindingsCount,
            Score = @Score,
            ComplianceRating = @ComplianceRating,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE AuditingId = @AuditingId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AuditingAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AuditingId=', @AuditingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditingId=', @AuditingId, N'; User=', SYSTEM_USER);
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
