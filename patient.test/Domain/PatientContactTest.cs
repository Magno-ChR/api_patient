using patient.domain.Entities.Contacts;
using patient.domain.Entities.Patients;
using patient.domain.Shared;

namespace patient.test;

public class PatientContactTest
{
    [Fact]
    public void PatientContactCreation_IsValid()
    {
        // Arrange
        var patientData = new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", new DateOnly(1990, 5, 20), "Ingeniero", "Católico", "Ninguna");

        patientData.AddContact(
            direction: "123 Main St",
            reference: "Near the park",
            phoneNumber: "555-1234",
            floor: "2",
            coords: "40.7128,-74.0060"
        );

        // Act
        var contact = patientData.Contacts.First();
        // Assert
        Assert.Equal("123 Main St", contact.Direction);
    }

    [Fact]
    public void PatientContactDeletion_IsValid()
    {
        // Arrange
        var patientData = new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", new DateOnly(1990, 5, 20), "Ingeniero", "Católico", "Ninguna");
        patientData.AddContact(
            direction: "123 Main St",
            reference: "Near the park",
            phoneNumber: "555-1234",
            floor: "2",
            coords: "40.7128,-74.0060"
        );
        // Act
        var contact = patientData.Contacts.First();
        patientData.RemoveContact(contact.Id, patientData.Id);
        // Assert
        Assert.False(patientData.Contacts.First().IsActive);
    }
}
