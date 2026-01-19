using Microsoft.Extensions.DependencyInjection;
using patient.domain;
using Xunit;

namespace patient.test.UnitTest.Domain;

public class DomainDependencyInjectionTests
{
    [Fact]
    public void AddDomain_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDomain();

        // Assert
        Assert.Same(services, result);
    }
}
