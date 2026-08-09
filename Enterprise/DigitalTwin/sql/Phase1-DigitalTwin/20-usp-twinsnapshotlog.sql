-- =============================================
-- Enterprise.DigitalTwin -- Phase1-DigitalTwin
-- Script:        20-usp-twinsnapshotlog.sql
-- Purpose:       usp_TwinSnapshotLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_TwinSnapshotLog_Record]
    @SimulatedAction  nvarchar(50) = NULL,
    @FacilityJson     nvarchar(max) = NULL,
    @EmployeeJson     nvarchar(max) = NULL,
    @MachineJson      nvarchar(max) = NULL,
    @HazardJson       nvarchar(max) = NULL,
    @WorkflowJson     nvarchar(max) = NULL,
    @EventJson        nvarchar(max) = NULL,
    @SimulatedAtUtc   datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityJson IS NOT NULL AND ISJSON(@FacilityJson) = 0
            THROW 50001, N'FacilityJson must be valid JSON when supplied.', 1;
        IF @EmployeeJson IS NOT NULL AND ISJSON(@EmployeeJson) = 0
            THROW 50001, N'EmployeeJson must be valid JSON when supplied.', 1;
        IF @MachineJson IS NOT NULL AND ISJSON(@MachineJson) = 0
            THROW 50001, N'MachineJson must be valid JSON when supplied.', 1;
        IF @HazardJson IS NOT NULL AND ISJSON(@HazardJson) = 0
            THROW 50001, N'HazardJson must be valid JSON when supplied.', 1;
        IF @WorkflowJson IS NOT NULL AND ISJSON(@WorkflowJson) = 0
            THROW 50001, N'WorkflowJson must be valid JSON when supplied.', 1;
        IF @EventJson IS NOT NULL AND ISJSON(@EventJson) = 0
            THROW 50001, N'EventJson must be valid JSON when supplied.', 1;

        INSERT INTO [DigitalTwin].[TwinSnapshotLog] (
            [SimulatedAction], [FacilityJson], [EmployeeJson], [MachineJson], [HazardJson], [WorkflowJson], [EventJson], [SimulatedAtUtc]
        )
        VALUES (
            @SimulatedAction, @FacilityJson, @EmployeeJson, @MachineJson, @HazardJson, @WorkflowJson, @EventJson, @SimulatedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextD0 nvarchar(200) = CONCAT(N'Params=SimulatedAction=', @SimulatedAction, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsD0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextD0, @ErrorDetailsD0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_TwinSnapshotLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [TwinSnapshotLogId], [SimulatedAction], [FacilityJson], [EmployeeJson],
               [MachineJson], [HazardJson], [WorkflowJson], [EventJson], [SimulatedAtUtc]
        FROM [DigitalTwin].[TwinSnapshotLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [SimulatedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextD1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsD1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextD1, @ErrorDetailsD1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
