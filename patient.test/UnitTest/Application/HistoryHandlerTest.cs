using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using patient.application.Histories.CreateHistory;
using patient.application.Histories.GetHistory;
using patient.application.Histories.UpdateHistory;
using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Histories;
using patient.domain.Results;
using Xunit;

namespace patient.test.UnitTest.Application;

public class HistoryHandlerTest
{
    [Fact]
    public async Task Handle_CreateHistoryCommand_IsValid()
    {
        // Arrange
        var historyRepositoryMock = new Mock<IHistoryRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateHistoryHandler(historyRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new CreateHistoryCommand(Guid.NewGuid(), Guid.NewGuid(), "Motivo", "Diagnostico", "Tratamiento");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        historyRepositoryMock.Verify(hr => hr.AddAsync(It.IsAny<History>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateBackgroundCommand_IsValid()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");

        var historyRepositoryMock = new Mock<IHistoryRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        historyRepositoryMock
            .Setup(hr => hr.GetByIdAsync(historyId, false))
            .ReturnsAsync(history);

        var handler = new CreateHistoryHandler(historyRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new CreateBackgroudCommand(historyId, "Detalle antecedente");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        historyRepositoryMock.Verify(hr => hr.UpdateAsync(It.IsAny<History>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateBackgroundCommand_WhenHistoryNotFound_ShouldFail()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var historyRepositoryMock = new Mock<IHistoryRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        historyRepositoryMock
            .Setup(hr => hr.GetByIdAsync(historyId, false))
            .ReturnsAsync((History?)null);

        var handler = new CreateHistoryHandler(historyRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateBackgroudCommand(historyId, "Detalle");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        historyRepositoryMock.Verify(hr => hr.UpdateAsync(It.IsAny<History>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetHistoryCommand_IsValid()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");

        var repoMock = new Mock<IHistoryRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(historyId, true))
                .ReturnsAsync(history);

        var handler = new GetHistoryHandler(repoMock.Object, uowMock.Object);

        // Act
        var result = await handler.Handle(new GetHistoryCommand(historyId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(history, result.Value);
    }

    [Fact]
    public async Task Handle_UpdateHistoryCommand_IsValid()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");

        var repoMock = new Mock<IHistoryRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(historyId, false))
                .ReturnsAsync(history);

        var handler = new UpdateHistoryHandler(repoMock.Object, uowMock.Object);

        var command = new UpdateHistoryCommand(historyId, "Nuevo motivo", "Nuevo D", "Nuevo T");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(history), Times.Once);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateBackgroundCommand_IsValid()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");
        history.AddBackground(new Background(Guid.NewGuid(), historyId, "detalle"));
        var backgroundId = history.Backgrounds.First().Id;

        var repoMock = new Mock<IHistoryRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(historyId, false))
                .ReturnsAsync(history);

        var handler = new UpdateHistoryHandler(repoMock.Object, uowMock.Object);

        var command = new UpdateBackgroundCommand(historyId, backgroundId, "nuevo detalle");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(history), Times.Once);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateBackgroundCommand_WhenBackgroundNotFound_ShouldFail()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");

        var repoMock = new Mock<IHistoryRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(historyId, false))
                .ReturnsAsync(history);

        var handler = new UpdateHistoryHandler(repoMock.Object, uowMock.Object);

        var command = new UpdateBackgroundCommand(historyId, Guid.NewGuid(), "nuevo detalle");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<History>()), Times.Never);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateEvolutionCommand_IsValid()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");
        history.AddEvolution(new Evolution(Guid.NewGuid(), historyId, "desc", "obs", "med"));
        var evolutionId = history.Evolutions.First().Id;

        var repoMock = new Mock<IHistoryRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(historyId, false))
                .ReturnsAsync(history);

        var handler = new UpdateHistoryHandler(repoMock.Object, uowMock.Object);

        var command = new UpdateEvolutionCommand(historyId, evolutionId, "n desc", "n obs", "n med");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(history), Times.Once);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateEvolutionCommand_WhenEvolutionNotFound_ShouldFail()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = new History(historyId, Guid.NewGuid(), Guid.NewGuid(), "R", "D", "T");

        var repoMock = new Mock<IHistoryRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        repoMock.Setup(r => r.GetByIdAsync(historyId, false))
                .ReturnsAsync(history);

        var handler = new UpdateHistoryHandler(repoMock.Object, uowMock.Object);

        var command = new UpdateEvolutionCommand(historyId, Guid.NewGuid(), "n desc", "n obs", "n med");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<History>()), Times.Never);
        uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
