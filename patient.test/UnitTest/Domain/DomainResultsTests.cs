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

    [Fact]
    public void Error_WithPlaceholders_ReplacesArgsInDescription()
    {
        var error = Error.Failure("x", "Hola {nombre}", "María");
        Assert.Contains("María", error.Description, StringComparison.Ordinal);
    }

    [Fact]
    public void Result_InvalidCombination_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Result(true, Error.Failure("c", "msg")));
        Assert.Throws<ArgumentException>(() => new Result(false, Error.None));
    }

    [Fact]
    public void ResultT_Value_OnFailure_ThrowsInvalidOperationException()
    {
        var r = Result.Failure<int>(Error.NotFound("n", "missing"));
        Assert.Throws<InvalidOperationException>(() => _ = r.Value);
    }

    [Fact]
    public void ResultT_ImplicitFromNull_IsFailureWithNullValueError()
    {
        Result<string> r = (string?)null;
        Assert.True(r.IsFailure);
        Assert.Equal(Error.NullValue, r.Error);
    }

    [Fact]
    public void ValidationError_FromResults_WhenAllSuccess_ReturnsEmptyErrorsArray()
    {
        var v = ValidationError.FromResults([Result.Success(), Result.Success()]);
        Assert.Empty(v.Errors);
    }
}
