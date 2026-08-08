-- =============================================
-- Shared.SafetyData -- Phase5-ComplianceRisk
-- Script:        20-usp-compliancescore-record.sql
-- Purpose:       Creates ComplianceRisk.usp_ComplianceScore_Record against
--                [PlantSafety.CoreDB] -- persists one computed
--                ComplianceScoreBuilder.Build(facility) output (Shared.
--                SafetyData.Abstractions.Models.Industries.ComplianceRisk\
--                ComplianceScore.cs) as a new history row for the facility.
--                Idempotent on (FacilityId, ComputedAtUtc) -- a re-delivered
--                event that recomputes the identical score at the identical
--                instant is a no-op, matching the [Orchestration] schema's
--                idempotent-on-EventId rule applied to this table's own key.
--
-- Idempotent (script itself): CREATE OR ALTER. Safe to re-run.
-- Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [ComplianceRisk].[usp_ComplianceScore_Record]
    @FacilityId             uniqueidentifier,
    @ComputedAtUtc          datetime2(3),
    @RequiredElementsCount  int,
    @CompletedElementsCount int,
    @MissingElementsCount   int,
    @PercentComplete        float,
    @ComplianceTier         nvarchar(30),
    @Notes                  nvarchar(400)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;
        IF @ComputedAtUtc IS NULL
            THROW 50001, N'ComputedAtUtc is required.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM [ComplianceRisk].[ComplianceScore] WITH (UPDLOCK, HOLDLOCK)
            WHERE [FacilityId] = @FacilityId AND [ComputedAtUtc] = @ComputedAtUtc
        )
        BEGIN
            INSERT INTO [ComplianceRisk].[ComplianceScore] (
                [FacilityId], [ComputedAtUtc], [RequiredElementsCount], [CompletedElementsCount],
                [MissingElementsCount], [PercentComplete], [ComplianceTier], [Notes]
            )
            VALUES (
                @FacilityId, @ComputedAtUtc, @RequiredElementsCount, @CompletedElementsCount,
                @MissingElementsCount, @PercentComplete, @ComplianceTier, @Notes
            );

            BEGIN TRY
                INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                    AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
                )
                VALUES (
                    N'ComplianceRisk.ComplianceScore', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId, N'; ComputedAtUtc=', @ComputedAtUtc, N'; ComplianceTier=', @ComplianceTier),
                    NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
                );
            END TRY
            BEGIN CATCH
            END CATCH;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext20 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; ComputedAtUtc=', @ComputedAtUtc, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails20 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext20, @ErrorDetails20, DB_NAME(), OBJECT_NAME(@@PROCID)
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
                N'ComplianceRisk.ComplianceScore', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext20, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
