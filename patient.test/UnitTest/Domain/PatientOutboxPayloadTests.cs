using patient.domain.Entities.Patients;
using patient.domain.Shared;

namespace patient.test.UnitTest.Domain;

public class PatientOutboxPayloadTests
{
	[Fact]
	public void PatientOutboxPayload_Equality_ByValue()
	{
		var id = Guid.NewGuid();
		var dob = new DateOnly(1990, 3, 15);
		var a = new PatientOutboxPayload(
			id, "Ana", "M", "López", BloodType.APositive, "123", dob, "Dev", "X", "Ninguna");
		var b = new PatientOutboxPayload(
			id, "Ana", "M", "López", BloodType.APositive, "123", dob, "Dev", "X", "Ninguna");

		Assert.Equal(a, b);
		Assert.True(a == b);
	}
}
