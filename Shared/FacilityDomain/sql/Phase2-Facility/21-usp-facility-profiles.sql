-- =============================================
-- Shared.FacilityDomain -- Phase2-Facility
-- Script:        21-usp-facility-profiles.sql
-- Purpose:       Replace-wholesale procs for the three FacilityDomain
--                profile child tables (HazardProfileItem/
--                RegulatoryProfileItem/ProgramProfileItem), mirroring
--                FacilityAggregate.UpdateHazardProfile/
--                UpdateRegulatoryProfile/UpdateProgramProfile's own
--                replace-not-merge semantics: delete every existing row for
--                the FacilityId then insert the new set, in one transaction
--                (XACT_ABORT). Item lists are passed as a delimited NVARCHAR
--                (caller splits its IReadOnlyList<string> into a
--                comma-separated string) -- consistent with this codebase's
--                no-TVP, plain-parameter convention seen throughout
--                Phase1-5.
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityHazardProfile_Replace]
    @FacilityId          uniqueidentifier,
    @HazardTypeCodesCsv  nvarchar(max), -- comma-separated Lookup.HazardType.Code values; '' = empty profile
    @LastUpdatedUtc      datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        BEGIN TRANSACTION;

        DELETE FROM [FacilityDomain].[FacilityHazardProfileItem] WHERE [FacilityId] = @FacilityId;

        INSERT INTO [FacilityDomain].[FacilityHazardProfileItem] ([FacilityId], [HazardTypeCode], [LastUpdatedUtc])
        SELECT @FacilityId, LTRIM(RTRIM(value)), @LastUpdatedUtc
        FROM STRING_SPLIT(@HazardTypeCodesCsv, ',')
        WHERE LTRIM(RTRIM(value)) <> N'';

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'FacilityDomain.FacilityHazardProfileItem', DB_NAME(), N'REPLACE', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
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
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext22, @ErrorDetails22, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityRegulatoryProfile_Replace]
    @FacilityId          uniqueidentifier,
    @RegulatorCodesCsv   nvarchar(max),
    @LastUpdatedUtc      datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        BEGIN TRANSACTION;
        DELETE FROM [FacilityDomain].[FacilityRegulatoryProfileItem] WHERE [FacilityId] = @FacilityId;
        INSERT INTO [FacilityDomain].[FacilityRegulatoryProfileItem] ([FacilityId], [RegulatorCode], [LastUpdatedUtc])
        SELECT @FacilityId, LTRIM(RTRIM(value)), @LastUpdatedUtc
        FROM STRING_SPLIT(@RegulatorCodesCsv, ',')
        WHERE LTRIM(RTRIM(value)) <> N'';
        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'FacilityDomain.FacilityRegulatoryProfileItem', DB_NAME(), N'REPLACE', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorContext23 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails23 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext23, @ErrorDetails23, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityProgramProfile_Replace]
    @FacilityId          uniqueidentifier,
    @ProgramCodesCsv     nvarchar(max),
    @LastUpdatedUtc      datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        BEGIN TRANSACTION;
        DELETE FROM [FacilityDomain].[FacilityProgramProfileItem] WHERE [FacilityId] = @FacilityId;
        INSERT INTO [FacilityDomain].[FacilityProgramProfileItem] ([FacilityId], [ProgramCode], [LastUpdatedUtc])
        SELECT @FacilityId, LTRIM(RTRIM(value)), @LastUpdatedUtc
        FROM STRING_SPLIT(@ProgramCodesCsv, ',')
        WHERE LTRIM(RTRIM(value)) <> N'';
        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'FacilityDomain.FacilityProgramProfileItem', DB_NAME(), N'REPLACE', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
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
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext24, @ErrorDetails24, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
