-- =============================================
-- Shared.SafetyData -- Phase4-Facilities
-- Script:        24-usp-facilitydashboardmetadata-upsert.sql
-- Purpose:       Creates Facilities.usp_FacilityDashboardMetadata_Upsert
--                against [PlantSafety.CoreDB] -- persists
--                FacilityDashboardMetadataBuilder.Build(FacilityProfile)'s
--                output (Shared.SafetyData.Abstractions.Models.Industries.
--                Facilities\FacilityDashboardMetadata.cs) for a given
--                FacilityId. Added per 03-table-facilitydashboardmetadata.sql
--                (schema-sentinel-agent's third [Facilities] table, not
--                originally named in this folder's own 00-create-schema.sql
--                description) -- additive.
--
-- COLUMN SHAPE: KeyIndicators/RequiredWidgets/RequiredCharts/RequiredAlerts
-- are persisted as ISJSON-checked NVARCHAR(MAX) array snapshot columns.
--
-- Idempotent (script itself): CREATE OR ALTER. True upsert keyed by
-- FacilityId (1:1 child of FacilityProfile, current-state cache). Safe to
-- re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Facilities].[usp_FacilityDashboardMetadata_Upsert]
    @FacilityId           uniqueidentifier,
    @KeyIndicatorsJson     nvarchar(max),
    @RequiredWidgetsJson   nvarchar(max),
    @RequiredChartsJson    nvarchar(max),
    @RequiredAlertsJson    nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;
        IF NOT EXISTS (SELECT 1 FROM [Facilities].[FacilityProfile] WHERE [FacilityId] = @FacilityId)
            THROW 50002, N'No FacilityProfile exists for the given FacilityId; upsert the profile first.', 1;
        IF ISJSON(@KeyIndicatorsJson) = 0
            THROW 50001, N'KeyIndicatorsJson must be valid JSON.', 1;
        IF ISJSON(@RequiredWidgetsJson) = 0
            THROW 50001, N'RequiredWidgetsJson must be valid JSON.', 1;
        IF ISJSON(@RequiredChartsJson) = 0
            THROW 50001, N'RequiredChartsJson must be valid JSON.', 1;
        IF ISJSON(@RequiredAlertsJson) = 0
            THROW 50001, N'RequiredAlertsJson must be valid JSON.', 1;

        MERGE [Facilities].[FacilityDashboardMetadata] AS target
        USING (SELECT @FacilityId AS FacilityId) AS source
            ON target.[FacilityId] = source.FacilityId
        WHEN MATCHED THEN
            UPDATE SET
                [KeyIndicatorsJson] = @KeyIndicatorsJson,
                [RequiredWidgetsJson] = @RequiredWidgetsJson,
                [RequiredChartsJson] = @RequiredChartsJson,
                [RequiredAlertsJson] = @RequiredAlertsJson,
                [UpdatedUtc] = SYSUTCDATETIME(),
                [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([FacilityId], [KeyIndicatorsJson], [RequiredWidgetsJson], [RequiredChartsJson], [RequiredAlertsJson])
            VALUES (@FacilityId, @KeyIndicatorsJson, @RequiredWidgetsJson, @RequiredChartsJson, @RequiredAlertsJson);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Facilities.FacilityDashboardMetadata', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'FacilityId=', @FacilityId),
                NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext24 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails24 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext24, @ErrorDetails24, DB_NAME(), OBJECT_NAME(@@PROCID)
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
                N'Facilities.FacilityDashboardMetadata', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext24, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
