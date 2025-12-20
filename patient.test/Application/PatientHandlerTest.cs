using Moq;
using patient.application.Patients.CreatePatient;
using patient.application.Patients.DeletePatient;
using patient.application.Patients.GetPatient;
using patient.application.Patients.UpdatePatient;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Shared;
using Xunit.Abstractions;


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

    [Fact]
    public async Task Handle_CreatePatientContactCommand_IsValid()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var patient = new Patient(
            patientId,
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

        var patientRepositoryMock = new Mock<IPatientRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        patientRepositoryMock
            .Setup(pr => pr.GetByIdAsync(patientId, false))
            .ReturnsAsync(patient);

        var handler = new CreatePatientHandler(
            patientRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        var command = new CreatePatientContactCommand(
            patientId,
            "Av. Siempre Viva 742",
            "777-9999",
            "2",
            "-17.7833,-63.1821",
            "Frente al colegio"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        patientRepositoryMock.Verify(
            pr => pr.GetByIdAsync(patientId, false),
            Times.Once
        );

        patientRepositoryMock.Verify(
            pr => pr.UpdateAsync(It.Is<Patient>(p => p.Contacts.Any())),
            Times.Once
        );

        unitOfWorkMock.Verify(
            uow => uow.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CreatePatientContactCommand_WhenPatientNotFound_ShouldFail()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var patientRepositoryMock = new Mock<IPatientRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        patientRepositoryMock
            .Setup(pr => pr.GetByIdAsync(patientId, false))
            .ReturnsAsync((Patient)null);

        var handler = new CreatePatientHandler(
            patientRepositoryMock.Object,
            unitOfWorkMock.Object
        );

        var command = new CreatePatientContactCommand(
            patientId,
            "Dirección",
            "555-0000",
            "1",
            "0,0",
            null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);

        patientRepositoryMock.Verify(
            pr => pr.UpdateAsync(It.IsAny<Patient>()),
            Times.Never
        );

        unitOfWorkMock.Verify(
            uow => uow.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_DeletePatientContactCommand_IsValid()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var patient = new Patient(
            patientId,
            "Juan",
            "Carlos",
            "Perez",
            BloodType.ONegative,
            "123",
            new DateOnly(1990, 1, 1),
            "Ing",
            "None",
            "None"
        );

        patient.AddContact("Dir", "Ref", "555", "1", "0,0");
        var contactId = patient.Contacts.First().Id;

        var repoMock = new Mock<IPatientRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(patientId, false))
                .ReturnsAsync(patient);

        var handler = new DeletePatientHandler(repoMock.Object, uowMock.Object);
        var command = new DeletePatientContactCommand(patientId, contactId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(patient), Times.Once);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeletePatientContactCommand_WhenPatientNotFound_ShouldFail()
    {
        // Arrange
        var repoMock = new Mock<IPatientRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), false))
                .ReturnsAsync((Patient)null);

        var handler = new DeletePatientHandler(repoMock.Object, uowMock.Object);

        // Act
        var result = await handler.Handle(
            new DeletePatientContactCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None
        );

        // Assert
        Assert.False(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Patient>()), Times.Never);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetPatientCommand_IsValid()
    {
        // Arrange
        var patient = new Patient(
            Guid.NewGuid(),
            "Juan",
            "Carlos",
            "Perez",
            BloodType.ONegative,
            "123",
            new DateOnly(1990, 1, 1),
            "Ing",
            "None",
            "None"
        );

        var repoMock = new Mock<IPatientRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(patient.Id, true))
                .ReturnsAsync(patient);

        var handler = new GetPatientHandler(repoMock.Object, uowMock.Object);

        // Act
        var result = await handler.Handle(
            new GetPatientCommand(patient.Id),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(patient, result.Value);
    }

    [Fact]
    public async Task Handle_GetPatientListCommand_IsValid()
    {
        // Arrange
        var patients = new[]
        {
            new Patient(
                Guid.NewGuid(),
                "Juan",
                "Carlos",
                "Perez",
                BloodType.ONegative,
                "123",
                new DateOnly(1990, 1, 1),
                "Ing",
                "None",
                "None"
            )
        };

        var pagedResult = new PagedResult<Patient>
        {
            Page = 1,
            PageSize = 10,
            TotalItems = patients.Count(),
            TotalPages = (int)Math.Ceiling(patients.Count() / (double)1),
            Items = patients.ToList()
        };

        var repoMock = new Mock<IPatientRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetPagedAsync(1, 10, null))
                .ReturnsAsync(pagedResult);

        var handler = new GetPatientHandler(repoMock.Object, uowMock.Object);

        // Act
        var result = await handler.Handle(
            new GetPatientListCommand(),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task Handle_UpdatePatientCommand_IsValid()
    {
        // Arrange
        var patient = new Patient(
            Guid.NewGuid(),
            "Juan",
            "Carlos",
            "Perez",
            BloodType.ONegative,
            "123",
            new DateOnly(1990, 1, 1),
            "Ing",
            "None",
            "None"
        );

        var repoMock = new Mock<IPatientRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(patient.Id, false))
                .ReturnsAsync(patient);

        var handler = new UpdatePatientHandler(repoMock.Object, uowMock.Object);

        var command = new UpdatePatientCommand(
            patient.Id,
            "Juan",
            "M",
            "Perez",
            BloodType.APositive,
            "999",
            new DateOnly(1991, 1, 1),
            "Dev",
            "None",
            "None"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(patient), Times.Once);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatePatientContactCommand_IsValid()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        var patient = new Patient(
            patientId,
            "Juan",
            "Carlos",
            "Perez",
            BloodType.ONegative,
            "123",
            new DateOnly(1990, 1, 1),
            "Ing",
            "None",
            "None"
        );

        patient.AddContact("Dir", "Ref", "555", "1", "0,0");
        var contact = patient.Contacts.First();

        var repoMock = new Mock<IPatientRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(patientId, false))
                .ReturnsAsync(patient);

        var handler = new UpdatePatientHandler(repoMock.Object, uowMock.Object);

        var command = new UpdatePatientContactCommand(
            patientId,
            contact.Id,
            "Nueva Dir",
            "Nueva Ref",
            "777",
            "2",
            "1,1"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(patient), Times.Once);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
