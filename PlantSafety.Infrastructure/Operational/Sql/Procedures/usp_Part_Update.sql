-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Part_Update.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Overwrites the mutable state of a Part aggregate root row (QuantityOnHand after
--                  a stock adjustment). Backs IPartRepository.UpdateAsync. @ETag enforces optimistic
--                  concurrency. Callers MUST invoke via ExecuteScalarAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_Part_Update]
    @PartId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @QuantityOnHand INT,
    @ReorderThreshold INT,
    @Location NVARCHAR(200),
    @UpdatedAtUtc DATETIME2(3),
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Operational].[Parts] WHERE PartId = @PartId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'Part not found or has been deleted.', 1;

        UPDATE [Operational].[Parts]
        SET
            QuantityOnHand = @QuantityOnHand,
            ReorderThreshold = @ReorderThreshold,
            Location = @Location,
            UpdatedAtUtc = @UpdatedAtUtc
        WHERE PartId = @PartId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Parts', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PartId=', @PartId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PartId=', @PartId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
