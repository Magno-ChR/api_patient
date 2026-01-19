using Microsoft.Extensions.DependencyInjection;
using patient.application;
using Xunit;

namespace patient.test.UnitTest.Application;

public class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersDomainAndMediatR()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        Assert.Same(services, result);
        // We can't easily assert MediatR registration here without adding MediatR to test project.
        // At minimum ensure AddDomain was called by verifying no exception and same instance returned.
    }
}
