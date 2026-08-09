// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           ErrorCategoryHttpMappingTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.ErrorHandling.Abstractions.Envelope;

using Xunit;

namespace Shared.ErrorHandling.Tests.Envelope;

public sealed class ErrorCategoryHttpMappingTests
{
    [Theory]
    [InlineData(ErrorCategory.Validation, 400)]
    [InlineData(ErrorCategory.Unauthorized, 401)]
    [InlineData(ErrorCategory.NotFound, 404)]
    [InlineData(ErrorCategory.Conflict, 409)]
    [InlineData(ErrorCategory.Dependency, 502)]
    [InlineData(ErrorCategory.Internal, 500)]
    public void ToHttpStatusCode_MapsEachCategoryDeterministically(ErrorCategory category, int expectedStatusCode)
    {
        ErrorCategoryHttpMapping.ToHttpStatusCode(category).Should().Be(expectedStatusCode);
    }
}
