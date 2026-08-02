-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_HumanFactorsAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a HumanFactors aggregate root row.
--                  Backs IHumanFactorsRepository.UpdateAsync. @TenantId is part of the WHERE
--                  clause and @ETag enforces optimistic concurrency against the native
--                  RowVersion column from day one, mirroring usp_AuditingAggregate_Update.sql.
--                  @RowsAffected is captured via @@ROWCOUNT immediately after the UPDATE (before
--                  the audit INSERT) and returned via SELECT -- callers MUST invoke this proc
--                  via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT
--                  ON makes ExecuteNonQuery always return -1, which would silently disable the
--                  @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactors].[usp_HumanFactorsAggregate_Update]
    @HumanFactorsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @FactorType NVARCHAR(100),
    @AffectedUserId UNIQUEIDENTIFIER = NULL,
    @SeverityRating INT,
    @Description NVARCHAR(2000),
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [HumanFactors].[HumanFactorsAggregate] WHERE HumanFactorsId = @HumanFactorsId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'HumanFactors record not found or has been deleted.', 1;

        UPDATE [HumanFactors].[HumanFactorsAggregate]
        SET
            Status = @Status,
            FactorType = @FactorType,
            AffectedUserId = @AffectedUserId,
            SeverityRating = @SeverityRating,
            Description = @Description,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE HumanFactorsId = @HumanFactorsId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'HumanFactorsAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'HumanFactorsId=', @HumanFactorsId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HumanFactorsId=', @HumanFactorsId, N'; User=', SYSTEM_USER);
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
