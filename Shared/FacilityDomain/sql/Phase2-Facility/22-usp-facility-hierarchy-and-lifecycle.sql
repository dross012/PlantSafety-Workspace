-- =============================================
-- Shared.FacilityDomain -- Phase2-Facility
-- Script:        22-usp-facility-hierarchy-and-lifecycle.sql
-- Purpose:       Add-Building/Add-Area/Add-Zone (mirrors FacilityAggregate.
--                AddBuilding/AddArea/AddZone) and Record-LifecycleEvent
--                (mirrors ActivateFacility/ShutdownFacility/ReopenFacility/
--                DecommissionFacility, each of which both updates
--                Facility.LifecycleStatus and appends a
--                FacilityLifecycleEvent row, in one transaction).
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityBuilding_Add]
    @BuildingId  uniqueidentifier,
    @FacilityId  uniqueidentifier,
    @Name        nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @BuildingId IS NULL OR @FacilityId IS NULL
            THROW 50001, N'BuildingId and FacilityId are required.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;

        INSERT INTO [FacilityDomain].[FacilityBuilding] ([BuildingId], [FacilityId], [Name])
        VALUES (@BuildingId, @FacilityId, @Name);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext25 nvarchar(200) = CONCAT(N'Params=BuildingId=', @BuildingId, N'; FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails25 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext25, @ErrorDetails25, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityArea_Add]
    @AreaId      uniqueidentifier,
    @BuildingId  uniqueidentifier,
    @Name        nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AreaId IS NULL OR @BuildingId IS NULL
            THROW 50001, N'AreaId and BuildingId are required.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;

        INSERT INTO [FacilityDomain].[FacilityArea] ([AreaId], [BuildingId], [Name])
        VALUES (@AreaId, @BuildingId, @Name);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext26 nvarchar(200) = CONCAT(N'Params=AreaId=', @AreaId, N'; BuildingId=', @BuildingId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails26 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext26, @ErrorDetails26, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityZone_Add]
    @ZoneId  uniqueidentifier,
    @AreaId  uniqueidentifier,
    @Name    nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ZoneId IS NULL OR @AreaId IS NULL
            THROW 50001, N'ZoneId and AreaId are required.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;

        INSERT INTO [FacilityDomain].[FacilityZone] ([ZoneId], [AreaId], [Name])
        VALUES (@ZoneId, @AreaId, @Name);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext27 nvarchar(200) = CONCAT(N'Params=ZoneId=', @ZoneId, N'; AreaId=', @AreaId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails27 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext27, @ErrorDetails27, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [FacilityDomain].[usp_FacilityLifecycleEvent_Record]
    @FacilityId       uniqueidentifier,
    @EventType        nvarchar(30), -- 'Activated'|'ShutDown'|'Reopened'|'Decommissioned'
    @NewStatus        nvarchar(30), -- Facility.LifecycleStatus's new value
    @Reason           nvarchar(500) = NULL,
    @OccurredAtUtc    datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;
        IF @EventType NOT IN (N'Activated', N'ShutDown', N'Reopened', N'Decommissioned')
            THROW 50001, N'EventType must be one of Activated/ShutDown/Reopened/Decommissioned.', 1;

        BEGIN TRANSACTION;

        UPDATE [FacilityDomain].[Facility]
        SET [LifecycleStatus] = @NewStatus, [UpdatedAtUtc] = @OccurredAtUtc,
            [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHERE [FacilityId] = @FacilityId;

        INSERT INTO [FacilityDomain].[FacilityLifecycleEvent] ([FacilityId], [EventType], [Reason], [OccurredAtUtc])
        VALUES (@FacilityId, @EventType, @Reason, @OccurredAtUtc);

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'FacilityDomain.FacilityLifecycleEvent', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId, N'; EventType=', @EventType), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorContext28 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; EventType=', @EventType, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails28 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext28, @ErrorDetails28, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
