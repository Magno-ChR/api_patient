using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Patients.Events;

namespace patient.test.UnitTest.Domain;

public class FoodPlanAndEntityTests
{
	[Fact]
	public void FoodPlan_Create_WithEmptyGuid_Throws()
	{
		var ex = Assert.Throws<ArgumentException>(() => FoodPlan.Create(Guid.Empty, "x"));
		Assert.Equal("id", ex.ParamName);
	}

	[Fact]
	public void FoodPlan_UpdateDetails_WithNullName_NormalizesToEmpty()
	{
		var plan = FoodPlan.Create(Guid.NewGuid(), "A");
		plan.UpdateDetails(null!);
		Assert.Equal(string.Empty, plan.Name);
	}

	[Fact]
	public void DomainEvent_DefaultConstructor_SetsIdAndOccurredOn()
	{
		var before = DateTime.UtcNow.AddSeconds(-1);
		var evt = new ContactCreateEvent(Guid.NewGuid());
		var after = DateTime.UtcNow.AddSeconds(1);

		Assert.NotEqual(Guid.Empty, evt.Id);
		Assert.InRange(evt.OccurredOn, before, after);
	}
}
