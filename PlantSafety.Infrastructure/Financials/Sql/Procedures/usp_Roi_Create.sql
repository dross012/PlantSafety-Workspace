-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Roi_Create.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Records a new Safety ROI calculation. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_Create]
(
    @TenantId              UNIQUEIDENTIFIER,
    @CorrelationId         UNIQUEIDENTIFIER,
    @ActorId               UNIQUEIDENTIFIER,
    @NowUtc                DATETIME2(3),
    @RoiCalculationId      UNIQUEIDENTIFIER,
    @SiteId                UNIQUEIDENTIFIER = NULL,
    @FiscalYear            INT,
    @TotalInvestment       DECIMAL(18,2),
    @EstimatedAvoidedCosts DECIMAL(18,2),
    @NetBenefit            DECIMAL(18,2),
    @RoiPercentage         DECIMAL(9,2),
    @CurrencyCode          NVARCHAR(3)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [Financials].[RoiCalculation]
            ([RoiCalculationId], [TenantId], [SiteId], [FiscalYear], [TotalInvestment],
             [EstimatedAvoidedCosts], [NetBenefit], [RoiPercentage], [CurrencyCode],
             [CalculatedAtUtc], [CorrelationId], [ActorId])
        VALUES
            (@RoiCalculationId, @TenantId, @SiteId, @FiscalYear, @TotalInvestment,
             @EstimatedAvoidedCosts, @NetBenefit, @RoiPercentage, @CurrencyCode,
             @NowUtc, @CorrelationId, @ActorId);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
