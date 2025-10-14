﻿using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.FoodPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Entities.Histories
{
    public class History : AggregateRoot
    {
        public Guid PatientId { get; private set; }
        public Guid FoodPlanId { get; private set; }
        public DateTime EntryDate { get; private set; }
        public string Reason { get; private set; }
        public string? Diagnostic { get; private set; }
        public string? Treatment { get; private set; }

        private readonly List<Background> _backgrounds = new();
        public IReadOnlyCollection<Background> Backgrounds => _backgrounds.AsReadOnly();

        private readonly List<Evolution> _evolutions = new();
        public IReadOnlyCollection<Evolution> Evolutions => _evolutions.AsReadOnly();

        //internal si no tiene validaciones y si tiene es public
        public History(Guid id, Guid patientId, Guid foodPlanId, string reason, string diagnostic, string treatment)
        : base(id)
        {
            FoodPlanId = foodPlanId;
            PatientId = patientId;
            EntryDate = DateTime.UtcNow;
            Reason = reason;
            Diagnostic = diagnostic;
            Treatment = treatment;
        }

        public static History Create(Guid patientId, Guid foodPlanId, string reason, string diagnostic, string treatment)
        {
            if (patientId == Guid.Empty)
                throw new ArgumentException("El ID del paciente no puede estar vacío");
            if (foodPlanId == Guid.Empty)
                throw new ArgumentException("El ID del plan alimenticio no puede estar vacío");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("La razón no puede estar vacía");
            return new History(Guid.NewGuid(), patientId, foodPlanId, reason, diagnostic ?? string.Empty, treatment ?? string.Empty);
        }

        public static void Update(History history, string reason, string diagnostic, string treatment)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("La razón no puede estar vacía");
            history.Reason = reason;
            history.Diagnostic = diagnostic ?? string.Empty;
            history.Treatment = treatment ?? string.Empty;
        }

        public void AddBackground(Background background)
        {
            if (background == null)
                throw new ArgumentNullException(nameof(background), "El antecedente no puede ser nulo");
            if (background.HistoryId != this.Id)
                throw new ArgumentException("El ID de la historia del antecedente no coincide con el ID de esta historia");
            _backgrounds.Add(background);
        }

        public void RemoveBackground(Background background)
        {
            if (background == null)
                throw new ArgumentNullException(nameof(background), "El antecedente no puede ser nulo");
            if (!_backgrounds.Contains(background))
                throw new ArgumentException("El antecedente no existe en esta historia");
            _backgrounds.Remove(background);
        }

        public void AddEvolution(Evolution evolution)
        {
            if (evolution == null)
                throw new ArgumentNullException(nameof(evolution), "La evolución no puede ser nula");
            if (evolution.HistoryId != this.Id)
                throw new ArgumentException("El ID de la historia de la evolución no coincide con el ID de esta historia");
            _evolutions.Add(evolution);
        }

        public void RemoveEvolution(Evolution evolution)
        {
            if (evolution == null)
                throw new ArgumentNullException(nameof(evolution), "La evolución no puede ser nula");
            if (!_evolutions.Contains(evolution))
                throw new ArgumentException("La evolución no existe en esta historia");
            _evolutions.Remove(evolution);
        }

        private History() : base() { }

    }
}
