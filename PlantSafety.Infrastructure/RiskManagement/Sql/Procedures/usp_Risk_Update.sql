-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risk_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates all mutable fields of a Risk aggregate root row. Backs
--                  IRiskRepository.UpdateAsync. HazardDomainCategory is intentionally not
--                  updated here -- it is an immutable Infrastructure-layer classification tag
--                  assigned once at creation. Additive-only evolution.
--  UPDATED:        2026-07-28 -- returns the post-update row's ETag (CONVERT(NVARCHAR(34),
--                  CAST(RowVersion AS BINARY(8)), 1)) via a trailing SELECT so callers have an
--                  up-to-date ETag immediately after update without a separate round-trip. Schema-
--                  only: no @ETag input parameter or conflict-rejection logic is introduced here --
--                  IRiskRepository.UpdateAsync has no ETag concept to enforce (see
--                  RiskDapperRepository.RiskRow.ETag remarks); the pre-existing IF NOT EXISTS/THROW
--                  50004 existence check above is unchanged.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_Update]
    @RiskId              UNIQUEIDENTIFIER,
    @Status              NVARCHAR(30),
    @Category            NVARCHAR(30),
    @CurrentSeverity     INT = NULL,
    @CurrentLikelihood   INT = NULL,
    @Priority            NVARCHAR(10) = NULL,
    @DocumentedAtUtc     DATETIME2(3) = NULL,
    @ResidualSeverity    INT = NULL,
    @ResidualLikelihood  INT = NULL,
    @LastValidatedAtUtc  DATETIME2(3) = NULL,
    @ReviewCadence       NVARCHAR(30),
    @LastReviewedAtUtc   DATETIME2(3) = NULL,
    @NextReviewDueUtc    DATETIME2(3) = NULL,
    @ReviewCount         INT,
    @RiskVersion         INT,
    @UpdatedBy           UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [RiskMgmt].[Risk] WHERE RiskId = @RiskId AND IsDeleted = 0)
            THROW 50004, N'Risk not found or has been deleted.', 1;

        UPDATE [RiskMgmt].[Risk]
        SET
            Status = @Status,
            Category = @Category,
            CurrentSeverity = @CurrentSeverity,
            CurrentLikelihood = @CurrentLikelihood,
            Priority = @Priority,
            DocumentedAtUtc = @DocumentedAtUtc,
            ResidualSeverity = @ResidualSeverity,
            ResidualLikelihood = @ResidualLikelihood,
            LastValidatedAtUtc = @LastValidatedAtUtc,
            ReviewCadence = @ReviewCadence,
            LastReviewedAtUtc = @LastReviewedAtUtc,
            NextReviewDueUtc = @NextReviewDueUtc,
            ReviewCount = @ReviewCount,
            RiskVersion = @RiskVersion,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE RiskId = @RiskId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Risk', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [RiskMgmt].[Risk]
        WHERE RiskId = @RiskId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskId=', @RiskId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
