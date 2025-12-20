using patient.domain.Entities.Patients;
using patient.domain.Shared;

namespace patient.test.UnitTest.Domain;

public class PatientTest
{
    [Fact]
    public void ItemCreation_IsValid()
    {
        // Arrange
        var patientData = new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", new DateOnly(2000, 5, 20), "Ingeniero", "Católico", "Ninguna");

        // Act
        var patient = patientData.Create(patientData.Id, patientData.FirstName, patientData.MiddleName, patientData.LastName, patientData.BloodType, patientData.DocumentNumber, patientData.DateOfBirth, patientData.Ocupation, patientData.Religion, patientData.Alergies);

        // Assert
        Assert.NotNull(patient);
        Assert.Equal(patientData.Id, patient.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ItemCreation_InvalidFirstName(string firstName)
    {
        // Arrange
        var patientData = new Patient(Guid.NewGuid(), firstName, "Carlos", "Pérez", BloodType.ONegative, "12345678", new DateOnly(1990, 5, 20), "Ingeniero", "Católico", "Ninguna");
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => patientData.Create(patientData.Id, patientData.FirstName, patientData.MiddleName, patientData.LastName, patientData.BloodType, patientData.DocumentNumber, patientData.DateOfBirth, patientData.Ocupation, patientData.Religion, patientData.Alergies));
        Assert.Equal("El nombre no puede estar vacío", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ItemCreation_NullDateOfBirth(string invalidDate)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            if (!DateOnly.TryParse(invalidDate, out var dob))
                throw new ArgumentException("La fecha de nacimiento no es válida");

            new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", dob, "Ingeniero", "Católico", "Ninguna");
        });

        Assert.Equal("La fecha de nacimiento no es válida", exception.Message);
    }

    [Theory]
    [InlineData("0001-01-01")]
    public void ItemCreation_InvalidDateOfBirth(string invalidDate)
    {
        var date = DateOnly.Parse(invalidDate);
        var patientData = new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", date, "Ingeniero", "Católico", "Ninguna");    
        var exception = Assert.Throws<ArgumentException>(() => 
            patientData.Create(patientData.Id, patientData.FirstName, patientData.MiddleName, patientData.LastName, patientData.BloodType, patientData.DocumentNumber, patientData.DateOfBirth, patientData.Ocupation, patientData.Religion, patientData.Alergies));

        Assert.Equal("La fecha de nacimiento no puede inferior a 150 años", exception.Message);
    }

    [Fact]
    public void ItemUpdate_IsValid()
    {
        // Arrange
        var patientData = new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", new DateOnly(1990, 5, 20), "Ingeniero", "Católico", "Ninguna");
        // Act
        var updatedPatient = patientData.Update(patientData, "Pedro", "Luis", "Gómez", BloodType.APositive, "87654321", new DateOnly(1985, 10, 15), "Médico", "Protestante", "Penicilina");
        // Assert
        Assert.NotNull(updatedPatient);
        Assert.Equal("Pedro", updatedPatient.FirstName);
        Assert.Equal("Luis", updatedPatient.MiddleName);
        Assert.Equal("Gómez", updatedPatient.LastName);
        Assert.Equal(BloodType.APositive, updatedPatient.BloodType);
        Assert.Equal("87654321", updatedPatient.DocumentNumber);
        Assert.Equal(new DateOnly(1985, 10, 15), updatedPatient.DateOfBirth);
        Assert.Equal("Médico", updatedPatient.Ocupation);
        Assert.Equal("Protestante", updatedPatient.Religion);
        Assert.Equal("Penicilina", updatedPatient.Alergies);
    }
}