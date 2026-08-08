-- =============================================
-- Shared.SafetyData -- Phase4-Facilities
-- Script:        22-usp-facilitycomplianceprofile-upsert.sql
-- Purpose:       Creates Facilities.usp_FacilityComplianceProfile_Upsert
--                against [PlantSafety.CoreDB] -- persists the flattened
--                scorecard summary from FacilityComplianceProfileBuilder.
--                Build (Shared.SafetyData.Abstractions.Models.Industries.
--                Facilities\FacilityComplianceProfile.cs): RequiredElements/
--                CompletedElements/MissingElements counts,
--                ComplianceScorePercent, and RiskLevel.
--
-- COLUMN SHAPE (per 02-table-facilitycomplianceprofile.sql's COLUMN SHAPE
-- note): the full RequiredElements/CompletedElements/MissingElements/
-- PriorityActions element lists and Notes are NOT persisted here -- each
-- element list is deterministically recomputable on demand from
-- FacilityProfile.RequiredPrograms (a pure function of the industry
-- binding, already cached on [Facilities].[FacilityProfile]) plus the
-- caller-supplied completedElementCodes set; only the scalar scorecard
-- summary is cached.
--
-- Idempotent (script itself): CREATE OR ALTER. True upsert keyed by
-- FacilityId (1:1 child of FacilityProfile, current-state cache, not
-- event-history). Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Facilities].[usp_FacilityComplianceProfile_Upsert]
    @FacilityId              uniqueidentifier,
    @RequiredElementsCount   int,
    @CompletedElementsCount  int,
    @MissingElementsCount    int,
    @ComplianceScorePercent  float,
    @RiskLevel               nvarchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;
        IF NOT EXISTS (SELECT 1 FROM [Facilities].[FacilityProfile] WHERE [FacilityId] = @FacilityId)
            THROW 50002, N'No FacilityProfile exists for the given FacilityId; upsert the profile first.', 1;

        MERGE [Facilities].[FacilityComplianceProfile] AS target
        USING (SELECT @FacilityId AS FacilityId) AS source
            ON target.[FacilityId] = source.FacilityId
        WHEN MATCHED THEN
            UPDATE SET
                [RequiredElementsCount] = @RequiredElementsCount,
                [CompletedElementsCount] = @CompletedElementsCount,
                [MissingElementsCount] = @MissingElementsCount,
                [ComplianceScorePercent] = @ComplianceScorePercent,
                [RiskLevel] = @RiskLevel,
                [UpdatedUtc] = SYSUTCDATETIME(),
                [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([FacilityId], [RequiredElementsCount], [CompletedElementsCount], [MissingElementsCount], [ComplianceScorePercent], [RiskLevel])
            VALUES (@FacilityId, @RequiredElementsCount, @CompletedElementsCount, @MissingElementsCount, @ComplianceScorePercent, @RiskLevel);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Facilities.FacilityComplianceProfile', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'FacilityId=', @FacilityId, N'; ComplianceScorePercent=', @ComplianceScorePercent),
                NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext22 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails22 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext22, @ErrorDetails22, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Facilities.FacilityComplianceProfile', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext22, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
