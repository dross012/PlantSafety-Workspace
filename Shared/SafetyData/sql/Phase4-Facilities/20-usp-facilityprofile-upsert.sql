-- =============================================
-- Shared.SafetyData -- Phase4-Facilities
-- Script:        20-usp-facilityprofile-upsert.sql
-- Purpose:       Creates Facilities.usp_FacilityProfile_Upsert against
--                [PlantSafety.CoreDB] -- persists the caller-supplied
--                identity fields (Name/IndustryName/Location) and raw Flags
--                bitmask of the most recent FacilityProfileBuilder.
--                Build(...) output for a given FacilityId (Shared.
--                SafetyData.Abstractions.Models.Industries.Facilities\
--                FacilityProfile.cs). True upsert keyed by FacilityId
--                (current-state cache, not an idempotent-on-EventId
--                append-only log) -- every call replaces the prior snapshot
--                for the same FacilityId.
--
-- COLUMN SHAPE (per 01-table-facilityprofile.sql's COLUMN SHAPE note):
-- Regulators/RequiredPrograms/RequiredPermits/RequiredTraining/
-- RequiredAudits/RequiredSops are NOT persisted as columns here -- each is
-- a pure, deterministic derivation of IndustryName alone (already fully
-- queryable through [SafetyTaxonomy]), so only the caller-supplied scalar
-- identity fields and the raw Flags bitmask are cached. Flags is BIGINT to
-- match the C# [Flags] enum's underlying storage.
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

CREATE OR ALTER PROCEDURE [Facilities].[usp_FacilityProfile_Upsert]
    @FacilityId   uniqueidentifier,
    @Name         nvarchar(200),
    @IndustryName nvarchar(200),
    @Location     nvarchar(400),
    @Flags        bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL OR @FacilityId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'FacilityId is required and must not be empty.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;
        IF @IndustryName IS NULL OR LTRIM(RTRIM(@IndustryName)) = N''
            THROW 50001, N'IndustryName is required.', 1;

        MERGE [Facilities].[FacilityProfile] AS target
        USING (SELECT @FacilityId AS FacilityId) AS source
            ON target.[FacilityId] = source.FacilityId
        WHEN MATCHED THEN
            UPDATE SET
                [Name] = @Name,
                [IndustryName] = @IndustryName,
                [Location] = @Location,
                [Flags] = @Flags,
                [UpdatedUtc] = SYSUTCDATETIME(),
                [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([FacilityId], [Name], [IndustryName], [Location], [Flags])
            VALUES (@FacilityId, @Name, @IndustryName, @Location, @Flags);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Facilities.FacilityProfile', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'FacilityId=', @FacilityId, N'; IndustryName=', @IndustryName),
                NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext20 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
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
                N'Facilities.FacilityProfile', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext20, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
