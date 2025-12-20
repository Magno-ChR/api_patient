using patient.domain.Entities.Histories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.test.UnitTest.Domain;

public class HistoryTest
{
    [Fact]
    public void HistoryCreate_IsValid()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var foodPlanId = Guid.NewGuid();
        var reason = "Consulta nutricional";
        var diagnostic = "Sobrepeso leve";
        var treatment = "Plan alimenticio balanceado";

        // Act
        var history = History.Create(patientId, foodPlanId, reason, diagnostic, treatment);

        // Assert
        Assert.NotNull(history);
        Assert.NotEqual(Guid.Empty, history.Id);
        Assert.Equal(patientId, history.PatientId);
        Assert.Equal(foodPlanId, history.FoodPlanId);
        Assert.Equal(reason, history.Reason);
        Assert.Equal(diagnostic, history.Diagnostic);
        Assert.Equal(treatment, history.Treatment);
    }

    [Fact]
    public void HistoryCreate_WhenDiagnosticAndTreatmentAreNull_ShouldSetEmptyString()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var foodPlanId = Guid.NewGuid();
        var reason = "Control mensual";

        // Act
        var history = History.Create(patientId, foodPlanId, reason, null, null);

        // Assert
        Assert.Equal(string.Empty, history.Diagnostic);
        Assert.Equal(string.Empty, history.Treatment);
    }

    [Fact]
    public void HistoryCreate_WhenPatientIdIsEmpty_ShouldThrowException()
    {
        // Arrange
        var foodPlanId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            History.Create(Guid.Empty, foodPlanId, "Motivo", "Diagnóstico", "Tratamiento")
        );

        Assert.Equal("El ID del paciente no puede estar vacío", exception.Message);
    }

    [Fact]
    public void HistoryCreate_WhenFoodPlanIdIsEmpty_ShouldThrowException()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            History.Create(patientId, Guid.Empty, "Motivo", "Diagnóstico", "Tratamiento")
        );

        Assert.Equal("El ID del plan alimenticio no puede estar vacío", exception.Message);
    }

    [Fact]
    public void HistoryCreate_WhenReasonIsEmpty_ShouldThrowException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var foodPlanId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            History.Create(patientId, foodPlanId, "   ", "Diagnóstico", "Tratamiento")
        );

        Assert.Equal("La razón no puede estar vacía", exception.Message);
    }

    [Fact]
    public void HistoryUpdate_IsValid()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo inicial",
            "Diagnóstico inicial",
            "Tratamiento inicial"
        );

        // Act
        History.Update(
            history,
            "Motivo actualizado",
            "Diagnóstico actualizado",
            "Tratamiento actualizado"
        );

        // Assert
        Assert.Equal("Motivo actualizado", history.Reason);
        Assert.Equal("Diagnóstico actualizado", history.Diagnostic);
        Assert.Equal("Tratamiento actualizado", history.Treatment);
    }

    [Fact]
    public void HistoryUpdate_WhenDiagnosticAndTreatmentAreNull_ShouldSetEmptyString()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo inicial",
            "Diagnóstico",
            "Tratamiento"
        );

        // Act
        History.Update(history, "Motivo actualizado", null, null);

        // Assert
        Assert.Equal("Motivo actualizado", history.Reason);
        Assert.Equal(string.Empty, history.Diagnostic);
        Assert.Equal(string.Empty, history.Treatment);
    }

    [Fact]
    public void HistoryUpdate_WhenReasonIsEmpty_ShouldThrowException()
    {
        // Arrange
        var history = History.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Motivo inicial",
            "Diagnóstico",
            "Tratamiento"
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            History.Update(history, " ", "Nuevo diagnóstico", "Nuevo tratamiento")
        );

        Assert.Equal("La razón no puede estar vacía", exception.Message);
    }

}
