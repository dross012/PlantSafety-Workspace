-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Permits_Update.sql
--  Created:        2026-07-30
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a Permits aggregate root row. Backs
--                  IPermitsRepository.UpdateAsync. @TenantId is part of the WHERE clause from day
--                  one (tenant-scoped write, no retrofit needed) and @ETag enforces optimistic
--                  concurrency against the native RowVersion column from day one, mirroring
--                  usp_PermitWorkflow_Update.sql / usp_Accident_Update.sql. @RowsAffected is
--                  captured via @@ROWCOUNT immediately after the UPDATE (before the audit INSERT,
--                  which would otherwise reset it) and returned via SELECT -- callers MUST invoke
--                  this proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since
--                  SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently
--                  disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permits_Update]
    @PermitsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @PermitType NVARCHAR(400),
    @PermitNumber NVARCHAR(400),
    @WorkDescription NVARCHAR(2000),
    @Location NVARCHAR(400),
    @HazardsIdentified NVARCHAR(2000),
    @RequestedByUserId UNIQUEIDENTIFIER,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL,
    @IssuedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @RejectedReason NVARCHAR(2000) = NULL,
    @RejectedAtUtc DATETIME2(3) = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @ExpiredAtUtc DATETIME2(3) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Permits] WHERE PermitsId = @PermitsId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'Permits not found or has been deleted.', 1;

        UPDATE [Compliance].[Permits]
        SET
            Status = @Status,
            PermitType = @PermitType,
            PermitNumber = @PermitNumber,
            WorkDescription = @WorkDescription,
            Location = @Location,
            HazardsIdentified = @HazardsIdentified,
            RequestedByUserId = @RequestedByUserId,
            ApprovedByUserId = @ApprovedByUserId,
            IssuedAtUtc = @IssuedAtUtc,
            ExpiresAtUtc = @ExpiresAtUtc,
            RejectedReason = @RejectedReason,
            RejectedAtUtc = @RejectedAtUtc,
            CompletedAtUtc = @CompletedAtUtc,
            ExpiredAtUtc = @ExpiredAtUtc,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE PermitsId = @PermitsId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Permits', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PermitsId=', @PermitsId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitsId=', @PermitsId, N'; User=', SYSTEM_USER);
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
