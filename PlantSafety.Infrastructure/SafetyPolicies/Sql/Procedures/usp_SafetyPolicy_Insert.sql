-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SafetyPolicy_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new SafetyPolicy aggregate root row. Backs
--                  ISafetyPolicyRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_SafetyPolicy_Insert]
    @SafetyPolicyId              UNIQUEIDENTIFIER,
    @TenantId                    UNIQUEIDENTIFIER,
    @SiteId                      UNIQUEIDENTIFIER = NULL,
    @Status                      NVARCHAR(30),
    @SafetyPolicyTitle           NVARCHAR(400),
    @PolicyNumber                NVARCHAR(100),
    @SafetyPolicyVersion         INT,
    @Category                    NVARCHAR(100),
    @Applicability               NVARCHAR(400),
    @OwnerId                     UNIQUEIDENTIFIER,
    @ApprovedBy                  UNIQUEIDENTIFIER = NULL,
    @ApprovedAtUtc               DATETIME2(3) = NULL,
    @EffectiveAtUtc              DATETIME2(3) = NULL,
    @ReviewCadence               NVARCHAR(30),
    @LastReviewedAtUtc           DATETIME2(3) = NULL,
    @NextReviewDueUtc            DATETIME2(3) = NULL,
    @ReviewRequired              BIT,
    @RequiredAcknowledgmentCount INT,
    @ReviewCount                 INT,
    @CreatedAtUtc                DATETIME2(3),
    @CreatedBy                   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyPolicies].[SafetyPolicy]
        (
            SafetyPolicyId, TenantId, SiteId, Status, SafetyPolicyTitle, PolicyNumber, SafetyPolicyVersion,
            Category, Applicability, OwnerId, ApprovedBy, ApprovedAtUtc, EffectiveAtUtc, ReviewCadence,
            LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, RequiredAcknowledgmentCount, ReviewCount,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @SafetyPolicyId, @TenantId, @SiteId, @Status, @SafetyPolicyTitle, @PolicyNumber, @SafetyPolicyVersion,
            @Category, @Applicability, @OwnerId, @ApprovedBy, @ApprovedAtUtc, @EffectiveAtUtc, @ReviewCadence,
            @LastReviewedAtUtc, @NextReviewDueUtc, @ReviewRequired, @RequiredAcknowledgmentCount, @ReviewCount,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'SafetyPolicy', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'SafetyPolicyId=', @SafetyPolicyId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SafetyPolicyId=', @SafetyPolicyId, N'; User=', SYSTEM_USER);
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
