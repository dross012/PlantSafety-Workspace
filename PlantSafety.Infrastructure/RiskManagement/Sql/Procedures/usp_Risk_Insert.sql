-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risk_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Risk aggregate root row. Backs IRiskRepository.AddAsync.
--                  @HazardDomainCategory is an Infrastructure-layer read-model classification tag
--                  (one of the 11 RiskManagement hazard-topic categories) derived by the Dapper
--                  repository from the aggregate's free-text HazardSource field -- the Risk
--                  aggregate itself has no such property (its own Category is the RiskLevel
--                  severity banding), so this is stored purely to back the 11 category
--                  Application-layer read-model repositories. TRY/CATCH with AuditDB/ErrorDB
--                  logging per the canonical write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_Insert]
    @RiskId               UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @SiteId               UNIQUEIDENTIFIER = NULL,
    @Status               NVARCHAR(30),
    @RiskTitle            NVARCHAR(400),
    @Category             NVARCHAR(30),
    @HazardDomainCategory NVARCHAR(100),
    @Location             NVARCHAR(400),
    @HazardSource         NVARCHAR(400),
    @OwnerId              UNIQUEIDENTIFIER,
    @CurrentSeverity      INT = NULL,
    @CurrentLikelihood    INT = NULL,
    @Priority             NVARCHAR(10) = NULL,
    @DocumentedAtUtc      DATETIME2(3) = NULL,
    @ResidualSeverity     INT = NULL,
    @ResidualLikelihood   INT = NULL,
    @LastValidatedAtUtc   DATETIME2(3) = NULL,
    @ReviewCadence        NVARCHAR(30),
    @LastReviewedAtUtc    DATETIME2(3) = NULL,
    @NextReviewDueUtc     DATETIME2(3) = NULL,
    @ReviewRequired       BIT,
    @ReviewCount          INT,
    @RiskVersion          INT,
    @CreatedAtUtc         DATETIME2(3),
    @CreatedBy            UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [RiskMgmt].[Risk]
        (
            RiskId, TenantId, SiteId, Status, RiskTitle, Category, HazardDomainCategory, Location, HazardSource,
            OwnerId, CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity,
            ResidualLikelihood, LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc,
            ReviewRequired, ReviewCount, RiskVersion, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @RiskId, @TenantId, @SiteId, @Status, @RiskTitle, @Category, @HazardDomainCategory, @Location, @HazardSource,
            @OwnerId, @CurrentSeverity, @CurrentLikelihood, @Priority, @DocumentedAtUtc, @ResidualSeverity,
            @ResidualLikelihood, @LastValidatedAtUtc, @ReviewCadence, @LastReviewedAtUtc, @NextReviewDueUtc,
            @ReviewRequired, @ReviewCount, @RiskVersion, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Risk', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
