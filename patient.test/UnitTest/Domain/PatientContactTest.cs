using patient.domain.Entities.Contacts;
using patient.domain.Entities.Patients;
using patient.domain.Shared;

namespace patient.test.UnitTest.Domain;

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
    public void PatientContactUpdate_IsValid()
    {
        // Arrange
        var patient = new Patient(
            Guid.NewGuid(),
            "Juan",
            "Carlos",
            "Pérez",
            BloodType.ONegative,
            "12345678",
            new DateOnly(1990, 5, 20),
            "Ingeniero",
            "Católico",
            "Ninguna"
        );

        patient.AddContact(
            direction: "123 Main St",
            reference: "Near the park",
            phoneNumber: "555-1234",
            floor: "2",
            coords: "40.7128,-74.0060"
        );

        var contact = patient.Contacts.First();

        // Act
        patient.UpdateContact(
            contact.Id,
            patient.Id,
            direction: "Av. Siempre Viva 742",
            reference: "Frente al colegio",
            phoneNumber: "777-9999",
            floor: "3",
            coords: "-17.7833,-63.1821"
        );

        // Assert
        var updatedContact = patient.Contacts.First();
        Assert.Equal("Av. Siempre Viva 742", updatedContact.Direction);
        Assert.Equal("Frente al colegio", updatedContact.Reference);
        Assert.Equal("777-9999", updatedContact.PhoneNumber);
        Assert.Equal("3", updatedContact.Floor);
        Assert.Equal("-17.7833,-63.1821", updatedContact.Coords);
    }

    [Fact]
    public void PatientContactUpdate_WhenContactDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var patient = new Patient(
            Guid.NewGuid(),
            "Juan",
            "Carlos",
            "Pérez",
            BloodType.ONegative,
            "12345678",
            new DateOnly(1990, 5, 20),
            "Ingeniero",
            "Católico",
            "Ninguna"
        );

        var fakeContactId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            patient.UpdateContact(
                fakeContactId,
                patient.Id,
                "Nueva dirección",
                "Nueva referencia",
                "000-0000",
                "1",
                "0,0"
            )
        );

        Assert.Equal("El contacto no existe para este paciente", exception.Message);
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
