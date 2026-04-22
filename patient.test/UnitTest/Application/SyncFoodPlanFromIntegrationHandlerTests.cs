using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using patient.application.Integration.FoodPlans;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;

namespace patient.test.UnitTest.Application;

public class SyncFoodPlanFromIntegrationHandlerTests
{
	[Fact]
	public async Task Handle_WhenCreated_AndNotExists_AddsAndCommits()
	{
		var id = Guid.NewGuid();
		var repo = new Mock<IFoodPlanRepository>();
		repo.Setup(r => r.GetByIdAsync(id, true)).ReturnsAsync((FoodPlan?)null);
		var uow = new Mock<IUnitOfWork>();

		var handler = new SyncFoodPlanFromIntegrationHandler(
			repo.Object,
			uow.Object,
			NullLogger<SyncFoodPlanFromIntegrationHandler>.Instance);

		await handler.Handle(
			new SyncFoodPlanFromIntegrationCommand
			{
				FoodPlanId = id,
				IsCreated = true,
				Name = "Plan Nuevo"
			},
			CancellationToken.None);

		repo.Verify(r => r.AddAsync(It.Is<FoodPlan>(f => f.Id == id && f.Name == "Plan Nuevo")), Times.Once);
		uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenCreated_AndAlreadyExists_SkipsWithoutCommit()
	{
		var id = Guid.NewGuid();
		var existing = FoodPlan.Create(id, "Viejo");
		var repo = new Mock<IFoodPlanRepository>();
		repo.Setup(r => r.GetByIdAsync(id, true)).ReturnsAsync(existing);
		var uow = new Mock<IUnitOfWork>();

		var handler = new SyncFoodPlanFromIntegrationHandler(
			repo.Object,
			uow.Object,
			NullLogger<SyncFoodPlanFromIntegrationHandler>.Instance);

		await handler.Handle(
			new SyncFoodPlanFromIntegrationCommand { FoodPlanId = id, IsCreated = true, Name = "Otro" },
			CancellationToken.None);

		repo.Verify(r => r.AddAsync(It.IsAny<FoodPlan>()), Times.Never);
		uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenUpdated_AndExists_UpdatesAndCommits()
	{
		var id = Guid.NewGuid();
		var plan = FoodPlan.Create(id, "Antes");
		var repo = new Mock<IFoodPlanRepository>();
		repo.Setup(r => r.GetByIdAsync(id, false)).ReturnsAsync(plan);
		var uow = new Mock<IUnitOfWork>();

		var handler = new SyncFoodPlanFromIntegrationHandler(
			repo.Object,
			uow.Object,
			NullLogger<SyncFoodPlanFromIntegrationHandler>.Instance);

		await handler.Handle(
			new SyncFoodPlanFromIntegrationCommand { FoodPlanId = id, IsCreated = false, Name = "Después" },
			CancellationToken.None);

		Assert.Equal("Después", plan.Name);
		repo.Verify(r => r.AddAsync(It.IsAny<FoodPlan>()), Times.Never);
		uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenUpdated_AndMissing_CreatesAndCommits()
	{
		var id = Guid.NewGuid();
		var repo = new Mock<IFoodPlanRepository>();
		repo.Setup(r => r.GetByIdAsync(id, false)).ReturnsAsync((FoodPlan?)null);
		var uow = new Mock<IUnitOfWork>();

		var handler = new SyncFoodPlanFromIntegrationHandler(
			repo.Object,
			uow.Object,
			NullLogger<SyncFoodPlanFromIntegrationHandler>.Instance);

		await handler.Handle(
			new SyncFoodPlanFromIntegrationCommand { FoodPlanId = id, IsCreated = false, Name = null },
			CancellationToken.None);

		repo.Verify(r => r.AddAsync(It.Is<FoodPlan>(f => f.Id == id && f.Name == string.Empty)), Times.Once);
		uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
