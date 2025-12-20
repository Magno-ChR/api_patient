using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Histories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.test.UnitTest.Domain;

public class HistoryEvolutionTest
{
    [Fact]
    public void AddEvolution_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var evolution = new Evolution(
            Guid.NewGuid(),
            history.Id,
            "Descripción inicial",
            "Observaciones",
            "Orden médica"
        );

        // Act
        history.AddEvolution(evolution);

        // Assert
        Assert.Single(history.Evolutions);
        Assert.Equal(evolution, history.Evolutions.First());
        Assert.Single(history.DomainEvents);
    }

    [Fact]
    public void AddEvolution_WhenEvolutionIsNull_ShouldThrowException()
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
            history.AddEvolution(null)
        );
    }

    [Fact]
    public void AddEvolution_WhenHistoryIdDoesNotMatch_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var evolution = new Evolution(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Descripción",
            "Observaciones",
            "Orden médica"
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            history.AddEvolution(evolution)
        );

        Assert.Equal(
            "El ID de la historia de la evolución no coincide con el ID de esta historia",
            exception.Message
        );
    }

    [Fact]
    public void UpdateEvolution_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var evolution = new Evolution(
            Guid.NewGuid(),
            history.Id,
            "Descripción inicial",
            "Observaciones iniciales",
            "Orden inicial"
        );

        history.AddEvolution(evolution);

        // Act
        history.UpdateEvolution(
            history.Id,
            evolution.Id,
            "Descripción actualizada",
            "Observaciones actualizadas",
            "Orden médica actualizada"
        );

        // Assert
        Assert.Equal("Descripción actualizada", evolution.Description);
        Assert.Equal("Observaciones actualizadas", evolution.Observations);
        Assert.Equal("Orden médica actualizada", evolution.MedicOrder);
    }

    [Fact]
    public void UpdateEvolution_WhenEvolutionDoesNotExist_ShouldThrowException()
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
            history.UpdateEvolution(
                history.Id,
                Guid.NewGuid(),
                "Descripción",
                "Observaciones",
                "Orden médica"
            )
        );

        Assert.Equal("La evolución no existe en esta historia", exception.Message);
    }

    [Fact]
    public void RemoveEvolution_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var evolution = new Evolution(
            Guid.NewGuid(),
            history.Id,
            "Descripción",
            "Observaciones",
            "Orden médica"
        );

        history.AddEvolution(evolution);

        // Act
        history.RemoveEvolution(evolution);

        // Assert
        Assert.Empty(history.Evolutions);
    }

    [Fact]
    public void RemoveEvolution_WhenEvolutionIsNull_ShouldThrowException()
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
            history.RemoveEvolution(null)
        );
    }

    [Fact]
    public void RemoveEvolution_WhenEvolutionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo",
            "Diagnóstico",
            "Tratamiento"
        );

        var evolution = new Evolution(
            Guid.NewGuid(),
            history.Id,
            "Descripción",
            "Observaciones",
            "Orden médica"
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            history.RemoveEvolution(evolution)
        );

        Assert.Equal("La evolución no existe en esta historia", exception.Message);
    }
}
