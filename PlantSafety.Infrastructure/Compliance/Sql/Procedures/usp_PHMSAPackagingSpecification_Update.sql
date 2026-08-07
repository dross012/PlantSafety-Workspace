-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Packaging
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAPackagingSpecification_Update.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PHMSA_PackagingSpecification aggregate
--                  root row. Backs IPackagingSpecificationRepository.UpdateAsync. @TenantId is part
--                  of the WHERE clause from day one (tenant-scoped write) and @ETag enforces
--                  optimistic concurrency against the native RowVersion column, mirroring
--                  usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT immediately
--                  after the UPDATE (before the audit INSERT, which would otherwise reset it) and
--                  returned via SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery always
--                  return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAPackagingSpecification_Update]
    @PackagingSpecificationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @DOTSpec NVARCHAR(20),
    @PerformanceTests NVARCHAR(MAX) = N'[]',
    @MaxCapacity DECIMAL(18,4),
    @CapacityUnit NVARCHAR(10),
    @ClosureInstructions NVARCHAR(MAX),
    @ReuseRestrictions NVARCHAR(MAX) = NULL,
    @AssociatedMaterialId UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Compliance].[PHMSA_PackagingSpecification] WHERE PackagingSpecificationId = @PackagingSpecificationId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'PHMSA_PackagingSpecification not found or has been deleted.', 1;

        UPDATE [Compliance].[PHMSA_PackagingSpecification]
        SET
            Status = @Status,
            DOTSpec = @DOTSpec,
            PerformanceTests = @PerformanceTests,
            MaxCapacity = @MaxCapacity,
            CapacityUnit = @CapacityUnit,
            ClosureInstructions = @ClosureInstructions,
            ReuseRestrictions = @ReuseRestrictions,
            AssociatedMaterialId = @AssociatedMaterialId,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE PackagingSpecificationId = @PackagingSpecificationId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PHMSA_PackagingSpecification', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PackagingSpecificationId=', @PackagingSpecificationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PackagingSpecificationId=', @PackagingSpecificationId, N'; User=', SYSTEM_USER);
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
