using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Histories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.test.Domain;

public class HistoryBackgroudTest
{
    [Fact]
    public void AddBackground_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var background = new Background(
            Guid.NewGuid(),
            history.Id,
            "Antecedente válido"
        );

        // Act
        history.AddBackground(background);

        // Assert
        Assert.Single(history.Backgrounds);
        Assert.Equal(background, history.Backgrounds.First());
        Assert.Single(history.DomainEvents);
    }

    [Fact]
    public void AddBackground_WhenBackgroundIsNull_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            history.AddBackground(null)
        );
    }

    [Fact]
    public void AddBackground_WhenHistoryIdDoesNotMatch_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var background = new Background(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Antecedente inválido"
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            history.AddBackground(background)
        );

        Assert.Equal("El ID de la historia del antecedente no coincide con el ID de esta historia", exception.Message);
    }

    [Fact]
    public void UpdateBackground_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var background = new Background(
            Guid.NewGuid(),
            history.Id,
            "Detalle inicial"
        );

        history.AddBackground(background);

        // Act
        history.UpdateBackground(history.Id, background.Id, "Detalle actualizado");

        // Assert
        Assert.Equal("Detalle actualizado", background.Description);
    }

    [Fact]
    public void UpdateBackground_WhenBackgroundDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            history.UpdateBackground(history.Id, Guid.NewGuid(), "Detalle")
        );

        Assert.Equal("El antecedente no existe en esta historia", exception.Message);
    }

    [Fact]
    public void RemoveBackground_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var background = new Background(
            Guid.NewGuid(),
            history.Id,
            "Antecedente"
        );

        history.AddBackground(background);

        // Act
        history.RemoveBackground(background);

        // Assert
        Assert.Empty(history.Backgrounds);
    }

    [Fact]
    public void RemoveBackground_WhenBackgroundIsNull_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            history.RemoveBackground(null)
        );
    }

    [Fact]
    public void RemoveBackground_WhenBackgroundDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var background = new Background(
            Guid.NewGuid(),
            history.Id,
            "Antecedente"
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            history.RemoveBackground(background)
        );

        Assert.Equal("El antecedente no existe en esta historia", exception.Message);
    }
}
