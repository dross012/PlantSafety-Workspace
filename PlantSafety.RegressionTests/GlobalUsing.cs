// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RegressionTests
//  Layer:          Tests (Regression)
//  Description:    Global usings for the regression suite (PLNT-REG-*).
//
//                  This suite exists to stop FIXED defects from coming back.
//                  Every test here corresponds to a specific bug that was
//                  present in this repository and has been repaired -- not to
//                  a hypothetical one. Each test names the defect it pins in
//                  its remarks, so a future failure tells the reader what
//                  actually broke rather than merely that an assertion moved.
//
//                  A test belongs here when all three hold:
//                    1. the defect was real and observed (not speculative),
//                    2. it would have shipped silently, and
//                    3. no existing suite would have caught it.
// ============================================================================
global using System.Text.RegularExpressions;
global using FluentAssertions;
global using Xunit;
