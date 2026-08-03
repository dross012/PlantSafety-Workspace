-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_GlobalSafetyInsights_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
--  SchemaVersion:  2026.1
--  Description:    Inserts a newly computed GlobalSafetyInsights snapshot. Backs
--                  IGlobalSafetyInsightsRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [GlobalSafetyInsights].[usp_GlobalSafetyInsights_Insert]
    @GlobalSafetyInsightsId          UNIQUEIDENTIFIER,
    @TenantId                        UNIQUEIDENTIFIER,
    @RegionCode                      NVARCHAR(30),
    @SiteIdsJson                     NVARCHAR(MAX),
    @WindowStartUtc                  DATETIME2(3),
    @WindowEndUtc                    DATETIME2(3),
    @Trir                            DECIMAL(18,4),
    @Ltir                            DECIMAL(18,4),
    @ExposureRate                    DECIMAL(18,4),
    @IncidentDensity                 DECIMAL(18,6),
    @MachineStateIncidentCorrelation FLOAT,
    @TotalRecordableIncidents        INT,
    @TotalLostTimeIncidents          INT,
    @TotalHoursWorked                DECIMAL(18,2),
    @RiskScoresJson                  NVARCHAR(MAX),
    @AnomaliesJson                   NVARCHAR(MAX),
    @RecommendationsJson             NVARCHAR(MAX),
    @ComputedAtUtc                   DATETIME2(3),
    @CreatedBy                       UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [GlobalSafetyInsights].[Snapshot]
        (
            GlobalSafetyInsightsId, TenantId, RegionCode, SiteIdsJson, WindowStartUtc, WindowEndUtc,
            Trir, Ltir, ExposureRate, IncidentDensity, MachineStateIncidentCorrelation,
            TotalRecordableIncidents, TotalLostTimeIncidents, TotalHoursWorked,
            RiskScoresJson, AnomaliesJson, RecommendationsJson, ComputedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @GlobalSafetyInsightsId, @TenantId, @RegionCode, @SiteIdsJson, @WindowStartUtc, @WindowEndUtc,
            @Trir, @Ltir, @ExposureRate, @IncidentDensity, @MachineStateIncidentCorrelation,
            @TotalRecordableIncidents, @TotalLostTimeIncidents, @TotalHoursWorked,
            @RiskScoresJson, @AnomaliesJson, @RecommendationsJson, @ComputedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Snapshot', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'GlobalSafetyInsightsId=', @GlobalSafetyInsightsId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'GlobalSafetyInsightsId=', @GlobalSafetyInsightsId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
