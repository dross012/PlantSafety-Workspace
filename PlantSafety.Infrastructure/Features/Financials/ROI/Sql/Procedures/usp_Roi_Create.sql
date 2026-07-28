-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Roi_Create.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Creates a SafetyROI calculation record. Backs
--                  IRoiRepository.CreateAsync. Additive-only evolution.
--  UPDATED:        2026-07-28 -- brought up to the canonical write-procedure template: added the
--                  nested BEGIN TRY/CATCH AuditLog INSERT (defensive, silently no-ops if
--                  [PlantSafety.AuditDB] is unreachable) and a trailing SELECT returning the
--                  freshly-inserted row's ETag (CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1)), matching usp_Risk_Insert/usp_Accident_Insert. Schema-only: no
--                  @ETag input parameter or conflict-rejection logic is introduced here --
--                  IRoiRepository.CreateAsync has no ETag concept to enforce (see
--                  RoiDapperRepository.RoiReadModel.ETag remarks).
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_Create]
    @TenantId               UNIQUEIDENTIFIER,
    @CorrelationId          UNIQUEIDENTIFIER,
    @ActorId                UNIQUEIDENTIFIER,
    @NowUtc                 DATETIME2(3),
    @RoiCalculationId       UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER = NULL,
    @FiscalYear             INT,
    @TotalInvestment        DECIMAL(18,2),
    @EstimatedAvoidedCosts  DECIMAL(18,2),
    @NetBenefit             DECIMAL(18,2),
    @RoiPercentage          DECIMAL(18,2),
    @CurrencyCode           NVARCHAR(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Financials].[Roi]
            (RoiCalculationId, TenantId, SiteId, FiscalYear, TotalInvestment, EstimatedAvoidedCosts, NetBenefit,
             RoiPercentage, CurrencyCode, CalculatedAtUtc, IsDeleted, CorrelationId, CreatedAtUtc, CreatedBy)
        VALUES
            (@RoiCalculationId, @TenantId, @SiteId, @FiscalYear, @TotalInvestment, @EstimatedAvoidedCosts, @NetBenefit,
             @RoiPercentage, @CurrencyCode, SYSDATETIMEOFFSET(), 0, @CorrelationId, @NowUtc, @ActorId);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Roi', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RoiCalculationId=', @RoiCalculationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Financials].[Roi]
        WHERE RoiCalculationId = @RoiCalculationId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; RoiCalculationId=', @RoiCalculationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
