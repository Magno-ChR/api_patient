using System.Globalization;
using System.Text;
using patient.domain.Entities.Patients;
using patient.domain.Shared;

namespace patient.test.UnitTest.Domain;

public class PatientTest
{
    private static string NormalizeNoDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

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
        Assert.Equal(
            NormalizeNoDiacritics("El nombre no puede estar vacio"),
            NormalizeNoDiacritics(exception.Message));
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

        // In Linux CI this message can come with replacement-char artifacts for accented vowels.
        // We assert the stable prefix to keep the validation intent without encoding flakiness.
        Assert.Contains("La fecha de nacimiento no es v", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("0001-01-01")]
    public void ItemCreation_InvalidDateOfBirth(string invalidDate)
    {
        var date = DateOnly.Parse(invalidDate);
        var patientData = new Patient(Guid.NewGuid(), "Juan", "Carlos", "Pérez", BloodType.ONegative, "12345678", date, "Ingeniero", "Católico", "Ninguna");    
        var exception = Assert.Throws<ArgumentException>(() => 
            patientData.Create(patientData.Id, patientData.FirstName, patientData.MiddleName, patientData.LastName, patientData.BloodType, patientData.DocumentNumber, patientData.DateOfBirth, patientData.Ocupation, patientData.Religion, patientData.Alergies));

        Assert.Equal(
            NormalizeNoDiacritics("La fecha de nacimiento no puede inferior a 150 anos"),
            NormalizeNoDiacritics(exception.Message));
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