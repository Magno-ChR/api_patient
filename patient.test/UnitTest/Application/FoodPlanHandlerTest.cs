using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using patient.application.FoodPlans.CreateFoodPlan;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;
using Xunit;

namespace patient.test.UnitTest.Application;

public class FoodPlanHandlerTest
{
    [Fact]
    public async Task Handle_CreateFoodPlanCommand_IsValid()
    {
        // Arrange
        var foodPlanRepositoryMock = new Mock<IFoodPlanRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateFoodPlandHandler(foodPlanRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateFoodPlanCommand("Plan A");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        foodPlanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FoodPlan>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
