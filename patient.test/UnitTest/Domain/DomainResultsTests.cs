using Xunit;
using patient.domain.Results;
using System;

namespace patient.test.UnitTest.Domain;

public class DomainResultsTests
{
    [Fact]
    public void DomainException_ShouldExposeError()
    {
        // Arrange
        var error = Error.Failure("code", "message");

        // Act
        var ex = new DomainException(error);

        // Assert
        Assert.Equal(error, ex.Error);
        Assert.IsType<DomainException>(ex);
    }

    [Fact]
    public void ValidationError_FromResults_ShouldAggregateErrors()
    {
        // Arrange
        var r1 = Result.Failure(Error.NotFound("a", "not found"));
        var r2 = Result.Failure(Error.Failure("b", "bad"));
        var r3 = Result.Success();

        // Act
        var validation = ValidationError.FromResults(new[] { r1, r2, r3 });

        // Assert
        Assert.NotNull(validation);
        Assert.Equal(2, validation.Errors.Length);
        Assert.Contains(validation.Errors, e => e.Code == "a");
        Assert.Contains(validation.Errors, e => e.Code == "b");
    }
}
