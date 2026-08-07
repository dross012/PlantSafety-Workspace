-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Placarding
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatContainer_Update.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PHMSA_HazmatContainer aggregate root
--                  row. Backs IHazmatContainerRepository.UpdateAsync. @TenantId is part of the
--                  WHERE clause from day one (tenant-scoped write) and @ETag enforces optimistic
--                  concurrency against the native RowVersion column, mirroring
--                  usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT immediately
--                  after the UPDATE (before the audit INSERT, which would otherwise reset it) and
--                  returned via SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery always
--                  return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatContainer_Update]
    @HazmatContainerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @ContainerType NVARCHAR(30),
    @MaterialId UNIQUEIDENTIFIER,
    @PackagingSpecId UNIQUEIDENTIFIER,
    @Markings NVARCHAR(MAX) = N'[]',
    @Labels NVARCHAR(MAX) = N'[]',
    @Placards NVARCHAR(MAX) = N'[]',
    @OrientationArrowsRequired BIT = 0,
    @CompatibilityGroup NVARCHAR(5) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Compliance].[PHMSA_HazmatContainer] WHERE HazmatContainerId = @HazmatContainerId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'PHMSA_HazmatContainer not found or has been deleted.', 1;

        UPDATE [Compliance].[PHMSA_HazmatContainer]
        SET
            Status = @Status,
            ContainerType = @ContainerType,
            MaterialId = @MaterialId,
            PackagingSpecId = @PackagingSpecId,
            Markings = @Markings,
            Labels = @Labels,
            Placards = @Placards,
            OrientationArrowsRequired = @OrientationArrowsRequired,
            CompatibilityGroup = @CompatibilityGroup,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE HazmatContainerId = @HazmatContainerId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PHMSA_HazmatContainer', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'HazmatContainerId=', @HazmatContainerId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HazmatContainerId=', @HazmatContainerId, N'; User=', SYSTEM_USER);
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
