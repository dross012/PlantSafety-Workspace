-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AiGovernance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AiGovernanceAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of an AiGovernance aggregate root row. Backs
--                  IAiGovernanceRepository.UpdateAsync. @TenantId is part of the WHERE clause and
--                  @ETag enforces optimistic concurrency against the native RowVersion column from
--                  day one, mirroring usp_AuditingAggregate_Update.sql. @RowsAffected is captured
--                  via @@ROWCOUNT immediately after the UPDATE (before the audit INSERT) and
--                  returned via SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery always
--                  return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AiGovernance].[usp_AiGovernanceAggregate_Update]
    @AiGovernanceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @SystemName NVARCHAR(200),
    @AiUseCase NVARCHAR(2000),
    @RiskClassification NVARCHAR(50),
    @OwnerUserId UNIQUEIDENTIFIER,
    @RequiresHumanOversight BIT,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [AiGovernance].[AiGovernanceAggregate] WHERE AiGovernanceId = @AiGovernanceId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'AiGovernance not found or has been deleted.', 1;

        UPDATE [AiGovernance].[AiGovernanceAggregate]
        SET
            Status = @Status,
            SystemName = @SystemName,
            AiUseCase = @AiUseCase,
            RiskClassification = @RiskClassification,
            OwnerUserId = @OwnerUserId,
            RequiresHumanOversight = @RequiresHumanOversight,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE AiGovernanceId = @AiGovernanceId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AiGovernanceAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AiGovernanceId=', @AiGovernanceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiGovernanceId=', @AiGovernanceId, N'; User=', SYSTEM_USER);
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
