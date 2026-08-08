-- =============================================
-- Shared.SafetyData -- Phase5-ComplianceRisk
-- Script:        26-usp-riskreadiness-record.sql
-- Purpose:       Creates ComplianceRisk.usp_RiskReadiness_Record against
--                [PlantSafety.CoreDB] -- persists one computed
--                RiskReadinessBuilder.Build(facility) output (Shared.
--                SafetyData.Abstractions.Models.Industries.ComplianceRisk\
--                RiskReadiness.cs) as a new history row for the facility.
--                Idempotent on (FacilityId, ComputedAtUtc), same rule as
--                usp_ComplianceScore_Record.
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

CREATE OR ALTER PROCEDURE [ComplianceRisk].[usp_RiskReadiness_Record]
    @FacilityId              uniqueidentifier,
    @ComputedAtUtc           datetime2(3),
    @HazardResponseReady     bit,
    @EmergencyResponseReady  bit,
    @EventRoutingReady       bit,
    @OverallReadinessPercent float
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
            FROM [ComplianceRisk].[RiskReadiness] WITH (UPDLOCK, HOLDLOCK)
            WHERE [FacilityId] = @FacilityId AND [ComputedAtUtc] = @ComputedAtUtc
        )
        BEGIN
            INSERT INTO [ComplianceRisk].[RiskReadiness] (
                [FacilityId], [ComputedAtUtc], [HazardResponseReady], [EmergencyResponseReady], [EventRoutingReady], [OverallReadinessPercent]
            )
            VALUES (
                @FacilityId, @ComputedAtUtc, @HazardResponseReady, @EmergencyResponseReady, @EventRoutingReady, @OverallReadinessPercent
            );

            BEGIN TRY
                INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                    AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
                )
                VALUES (
                    N'ComplianceRisk.RiskReadiness', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'FacilityId=', @FacilityId, N'; ComputedAtUtc=', @ComputedAtUtc, N'; OverallReadinessPercent=', @OverallReadinessPercent),
                    NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
                );
            END TRY
            BEGIN CATCH
            END CATCH;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext26 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; ComputedAtUtc=', @ComputedAtUtc, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails26 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext26, @ErrorDetails26, DB_NAME(), OBJECT_NAME(@@PROCID)
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
                N'ComplianceRisk.RiskReadiness', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext26, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
