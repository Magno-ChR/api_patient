using Moq;
using patient.application.Patients.CreatePatient;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Shared;


namespace patient.test.Application;

public class PatientHandlerTest
{
    [Fact]
    public async void Handle_CreatePatientCommand_IsValid()
    {
        // Arrange
        var patientRepositoryMock = new Mock<IPatientRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreatePatientHandler(patientRepositoryMock.Object, unitOfWorkMock.Object);
        var createPatientCommand = new CreatePatientCommand("Jhon", "M", "Doe", BloodType.OPositive, "123456789", new DateOnly(1990, 1, 1), "Engineer", "None", "Peanuts");

        // Act
        var result = await handler.Handle(createPatientCommand, CancellationToken.None);
        // Assert
        Assert.True(result.IsSuccess);
        //patientRepositoryMock.Verify(pr => pr.AddAsync(It.Is<Patient>(i => i.));
        patientRepositoryMock.Verify(pr => pr.AddAsync(It.IsAny<Patient>()));
    }
}
