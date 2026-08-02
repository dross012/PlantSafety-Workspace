-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorVisitor
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ContractorVisitorAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a ContractorVisitor aggregate root row.
--                  Backs IContractorVisitorRepository.UpdateAsync. @TenantId is part of the WHERE
--                  clause and @ETag enforces optimistic concurrency against the native RowVersion
--                  column from day one. @RowsAffected is captured via @@ROWCOUNT immediately after
--                  the UPDATE (before the audit INSERT) and returned via SELECT -- callers MUST
--                  invoke this proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ContractorVisitor].[usp_ContractorVisitorAggregate_Update]
    @ContractorVisitorId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @FullName NVARCHAR(200),
    @CompanyName NVARCHAR(200),
    @VisitorType NVARCHAR(50),
    @PurposeOfVisit NVARCHAR(2000),
    @HostUserId UNIQUEIDENTIFIER,
    @ScheduledArrivalUtc DATETIME2(3) = NULL,
    @ActualArrivalUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [ContractorVisitor].[ContractorVisitorAggregate] WHERE ContractorVisitorId = @ContractorVisitorId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'ContractorVisitor not found or has been deleted.', 1;

        UPDATE [ContractorVisitor].[ContractorVisitorAggregate]
        SET
            Status = @Status,
            FullName = @FullName,
            CompanyName = @CompanyName,
            VisitorType = @VisitorType,
            PurposeOfVisit = @PurposeOfVisit,
            HostUserId = @HostUserId,
            ScheduledArrivalUtc = @ScheduledArrivalUtc,
            ActualArrivalUtc = @ActualArrivalUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE ContractorVisitorId = @ContractorVisitorId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ContractorVisitorAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ContractorVisitorId=', @ContractorVisitorId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; User=', SYSTEM_USER);
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
