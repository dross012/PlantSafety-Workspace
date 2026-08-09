-- ============================================================================
--  Project:        Shared.EmployeeDomain
--  Module:         Phase1-Employee
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [EmployeeDomain] schema -- the SQL surface
--                  for Shared.EmployeeDomain.Abstractions.Aggregates.
--                  EmployeeAggregate/EmployeeState (Path C.1): training
--                  profile, certifications, hazard exposure records, injury
--                  profile, deterministic safety score.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup\00-create-schema.sql DATABASE PLACEMENT ASSUMPTION note
--  (same rationale applies to every new Path B/C bounded context in this
--  task).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EmployeeDomain')
BEGIN
    EXEC('CREATE SCHEMA [EmployeeDomain] AUTHORIZATION dbo;');
END
GO
