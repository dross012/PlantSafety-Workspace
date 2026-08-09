-- =============================================
-- Shared.FacilityDomain -- Phase2-Facility
-- Script:        20-usp-facility-upsert.sql
-- Purpose:       Creates FacilityDomain.usp_Facility_Upsert and
--                FacilityDomain.usp_Facility_GetById against
--                [PlantSafety.CoreDB]. Mirrors
--                Facilities.usp_FacilityProfile_Upsert's structure/error
--                handling exactly.
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_Facility_Upsert]
    @FacilityId       uniqueidentifier,
    @Name             nvarchar(200),
    @IndustryName     nvarchar(200),
    @NaicsCode        nvarchar(20) = NULL,
    @AddressLine1     nvarchar(200),
    @City             nvarchar(100),
    @StateOrProvince  nvarchar(100),
    @PostalCode       nvarchar(20),
    @CountryCode      nvarchar(10),
    @LifecycleStatus  nvarchar(30),
    @CreatedAtUtc     datetime2(3),
    @UpdatedAtUtc     datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL OR @FacilityId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'FacilityId is required and must not be empty.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;

        MERGE [FacilityDomain].[Facility] AS target
        USING (SELECT @FacilityId AS FacilityId) AS source
            ON target.[FacilityId] = source.FacilityId
        WHEN MATCHED THEN
            UPDATE SET
                [Name] = @Name, [IndustryName] = @IndustryName, [NaicsCode] = @NaicsCode,
                [AddressLine1] = @AddressLine1, [City] = @City, [StateOrProvince] = @StateOrProvince,
                [PostalCode] = @PostalCode, [CountryCode] = @CountryCode,
                [LifecycleStatus] = @LifecycleStatus, [UpdatedAtUtc] = @UpdatedAtUtc,
                [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([FacilityId], [Name], [IndustryName], [NaicsCode], [AddressLine1], [City],
                    [StateOrProvince], [PostalCode], [CountryCode], [LifecycleStatus], [CreatedAtUtc], [UpdatedAtUtc])
            VALUES (@FacilityId, @Name, @IndustryName, @NaicsCode, @AddressLine1, @City,
                    @StateOrProvince, @PostalCode, @CountryCode, @LifecycleStatus, @CreatedAtUtc, @UpdatedAtUtc);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'FacilityDomain.Facility', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId, N'; IndustryName=', @IndustryName), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
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
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext20, @ErrorDetails20, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'FacilityDomain.Facility', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    NULL, NULL, @ErrorContext20, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_Facility_GetById]
    @FacilityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (N'FacilityDomain.Facility', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId), CONCAT(N'Viewer access via ', OBJECT_NAME(@@PROCID)), N'INFO', OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT [FacilityId], [Name], [IndustryName], [NaicsCode], [AddressLine1], [City], [StateOrProvince],
               [PostalCode], [CountryCode], [LifecycleStatus], [CreatedAtUtc], [UpdatedAtUtc]
        FROM [FacilityDomain].[Facility]
        WHERE [FacilityId] = @FacilityId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext21 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails21 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext21, @ErrorDetails21, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
