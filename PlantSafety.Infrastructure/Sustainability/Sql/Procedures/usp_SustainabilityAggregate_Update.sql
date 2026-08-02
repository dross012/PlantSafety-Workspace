-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SustainabilityAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a Sustainability aggregate root row.
--                  Backs ISustainabilityRepository.UpdateAsync. @TenantId is part of the WHERE
--                  clause and @ETag enforces optimistic concurrency against the native RowVersion
--                  column from day one, mirroring usp_AuditingAggregate_Update.sql.
--                  @RowsAffected is captured via @@ROWCOUNT immediately after the UPDATE (before
--                  the audit INSERT) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the
--                  @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Sustainability].[usp_SustainabilityAggregate_Update]
    @SustainabilityId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @InitiativeName NVARCHAR(400),
    @Category NVARCHAR(100),
    @TargetValue DECIMAL(18,4) = NULL,
    @OwnerUserId UNIQUEIDENTIFIER,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Sustainability].[SustainabilityAggregate] WHERE SustainabilityId = @SustainabilityId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'Sustainability initiative not found or has been deleted.', 1;

        UPDATE [Sustainability].[SustainabilityAggregate]
        SET
            Status = @Status,
            InitiativeName = @InitiativeName,
            Category = @Category,
            TargetValue = @TargetValue,
            OwnerUserId = @OwnerUserId,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE SustainabilityId = @SustainabilityId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'SustainabilityAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'SustainabilityId=', @SustainabilityId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SustainabilityId=', @SustainabilityId, N'; User=', SYSTEM_USER);
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
