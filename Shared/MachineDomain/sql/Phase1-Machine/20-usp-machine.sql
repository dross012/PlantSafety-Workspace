-- =============================================
-- Shared.MachineDomain -- Phase1-Machine
-- Script:        20-usp-machine.sql
-- Purpose:       usp_Machine_Upsert / GetById, usp_MachineFailureRecord_Record,
--                usp_MachineMaintenanceProfile_Replace.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [MachineDomain].[usp_Machine_Upsert]
    @MachineId                     uniqueidentifier,
    @Name                          nvarchar(200),
    @MachineType                   nvarchar(30),
    @FacilityId                    uniqueidentifier,
    @AreaDescription                nvarchar(400),
    @LastMaintenanceAtUtc           datetime2(3) = NULL,
    @NextScheduledMaintenanceAtUtc  datetime2(3) = NULL,
    @MaintenanceIntervalDays        int,
    @MaintenanceProfileUpdatedAtUtc datetime2(3),
    @SafetyScoreValue               float,
    @SafetyScoreComputedAtUtc       datetime2(3),
    @CreatedAtUtc                   datetime2(3),
    @UpdatedAtUtc                   datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @MachineId IS NULL OR @MachineId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'MachineId is required and must not be empty.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        MERGE [MachineDomain].[Machine] AS target
        USING (SELECT @MachineId AS MachineId) AS source
            ON target.[MachineId] = source.MachineId
        WHEN MATCHED THEN
            UPDATE SET [Name] = @Name, [MachineType] = @MachineType, [FacilityId] = @FacilityId,
                [AreaDescription] = @AreaDescription, [LastMaintenanceAtUtc] = @LastMaintenanceAtUtc,
                [NextScheduledMaintenanceAtUtc] = @NextScheduledMaintenanceAtUtc,
                [MaintenanceIntervalDays] = @MaintenanceIntervalDays,
                [MaintenanceProfileUpdatedAtUtc] = @MaintenanceProfileUpdatedAtUtc,
                [SafetyScoreValue] = @SafetyScoreValue, [SafetyScoreComputedAtUtc] = @SafetyScoreComputedAtUtc,
                [UpdatedAtUtc] = @UpdatedAtUtc, [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([MachineId], [Name], [MachineType], [FacilityId], [AreaDescription], [LastMaintenanceAtUtc],
                    [NextScheduledMaintenanceAtUtc], [MaintenanceIntervalDays], [MaintenanceProfileUpdatedAtUtc],
                    [SafetyScoreValue], [SafetyScoreComputedAtUtc], [CreatedAtUtc], [UpdatedAtUtc])
            VALUES (@MachineId, @Name, @MachineType, @FacilityId, @AreaDescription, @LastMaintenanceAtUtc,
                    @NextScheduledMaintenanceAtUtc, @MaintenanceIntervalDays, @MaintenanceProfileUpdatedAtUtc,
                    @SafetyScoreValue, @SafetyScoreComputedAtUtc, @CreatedAtUtc, @UpdatedAtUtc);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'MachineDomain.Machine', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'MachineId=', @MachineId), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext50 nvarchar(200) = CONCAT(N'Params=MachineId=', @MachineId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails50 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext50, @ErrorDetails50, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [MachineDomain].[usp_Machine_GetById]
    @MachineId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @MachineId IS NULL
            THROW 50001, N'MachineId is required.', 1;

        SELECT [MachineId], [Name], [MachineType], [FacilityId], [AreaDescription], [LastMaintenanceAtUtc],
               [NextScheduledMaintenanceAtUtc], [MaintenanceIntervalDays], [MaintenanceProfileUpdatedAtUtc],
               [SafetyScoreValue], [SafetyScoreComputedAtUtc], [CreatedAtUtc], [UpdatedAtUtc]
        FROM [MachineDomain].[Machine]
        WHERE [MachineId] = @MachineId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext51 nvarchar(200) = CONCAT(N'Params=MachineId=', @MachineId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails51 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext51, @ErrorDetails51, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [MachineDomain].[usp_MachineFailureRecord_Record]
    @MachineId                 uniqueidentifier,
    @Description                nvarchar(500),
    @ResultingHazardTypeCode    nvarchar(30),
    @FailedAtUtc                datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @MachineId IS NULL
            THROW 50001, N'MachineId is required.', 1;

        INSERT INTO [MachineDomain].[MachineFailureRecord]
            ([MachineId], [Description], [ResultingHazardTypeCode], [FailedAtUtc])
        VALUES (@MachineId, @Description, @ResultingHazardTypeCode, @FailedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext52 nvarchar(200) = CONCAT(N'Params=MachineId=', @MachineId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails52 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext52, @ErrorDetails52, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
