-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ManagementOfChange_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a ManagementOfChange aggregate root row.
--                  Backs IManagementOfChangeRepository.UpdateAsync. @TenantId is part of the WHERE
--                  clause from day one (tenant-scoped write, no retrofit needed) and @ETag enforces
--                  optimistic concurrency against the native RowVersion column from day one,
--                  mirroring usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT
--                  immediately after the UPDATE (before the audit INSERT, which would otherwise
--                  reset it) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes
--                  ExecuteNonQuery always return -1, which would silently disable the @ETag
--                  concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_ManagementOfChange_Update]
    @ManagementOfChangeId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ChangeTitle NVARCHAR(400),
    @ChangeType NVARCHAR(100),
    @Justification NVARCHAR(2000),
    @RiskLevel NVARCHAR(50),
    @RequestedByUserId UNIQUEIDENTIFIER,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL,
    @ApprovalStatus NVARCHAR(50),
    @ImplementationAtUtc DATETIME2(3) = NULL,
    @ReviewDueAtUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Compliance].[ManagementOfChange] WHERE ManagementOfChangeId = @ManagementOfChangeId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'ManagementOfChange not found or has been deleted.', 1;

        UPDATE [Compliance].[ManagementOfChange]
        SET
            Status = @Status,
            ChangeTitle = @ChangeTitle,
            ChangeType = @ChangeType,
            Justification = @Justification,
            RiskLevel = @RiskLevel,
            RequestedByUserId = @RequestedByUserId,
            ApprovedByUserId = @ApprovedByUserId,
            ApprovalStatus = @ApprovalStatus,
            ImplementationAtUtc = @ImplementationAtUtc,
            ReviewDueAtUtc = @ReviewDueAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE ManagementOfChangeId = @ManagementOfChangeId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ManagementOfChange', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; User=', SYSTEM_USER);
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
