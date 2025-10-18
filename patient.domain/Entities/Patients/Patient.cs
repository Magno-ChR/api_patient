﻿using patient.domain.Abstractions;
using patient.domain.Entities.Contacts;
using patient.domain.Shared;


namespace patient.domain.Entities.Patients;

public class Patient : AggregateRoot
{
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public BloodType BloodType { get; private set; }
    public string DocumentNumber { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public string Ocupation { get; private set; }
    public string Religion { get; private set; }
    public string Alergies { get; private set; }

    private readonly List<Contact> _contacts = new();
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();


    public Patient(Guid id, string firstName, string middleName, string lastName, BloodType bloodType, string documentNumber, DateOnly dateOfBirth, string ocupation, string religion, string alergies) : base(id)
    {      
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        BloodType = bloodType;
        DocumentNumber = documentNumber;
        DateOfBirth = dateOfBirth;
        Ocupation = ocupation;
        Religion = religion;
        Alergies = alergies;
    }

    public Patient Create(Guid id, string firstName, string middleName, string lastName, BloodType bloodType, string documentNumber, DateOnly dateOfBirth, string ocupation, string religion, string alergies)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("El nombre no puede estar vacío");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("El apellido no puede estar vacío");
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new ArgumentException("El número de documento no puede estar vacío");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("La fecha de nacimiento no puede ser en el futuro");
        return new Patient(id, firstName, middleName ?? string.Empty, lastName, bloodType, documentNumber, dateOfBirth, ocupation ?? string.Empty, religion ?? string.Empty, alergies ?? string.Empty);
    }

    public Patient Update(Patient patient, string firstName, string middleName, string lastName, BloodType bloodType, string documentNumber, DateOnly dateOfBirth, string ocupation, string religion, string alergies)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("El nombre no puede estar vacío");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("El apellido no puede estar vacío");
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new ArgumentException("El número de documento no puede estar vacío");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("La fecha de nacimiento no puede ser en el futuro");
        patient.FirstName = firstName;
        patient.MiddleName = middleName ?? string.Empty;
        patient.LastName = lastName;
        patient.BloodType = bloodType;
        patient.DocumentNumber = documentNumber;
        patient.DateOfBirth = dateOfBirth;
        patient.Ocupation = ocupation ?? string.Empty;
        patient.Religion = religion ?? string.Empty;
        patient.Alergies = alergies ?? string.Empty;

        return patient;
    }

    public void AddContact(string direction, string reference, string phoneNumber, string floor, string coords)
    {
        var contact = new Contact(Guid.NewGuid(), this.Id, direction, reference, phoneNumber, floor, coords);
        _contacts.Add(contact);
    }

    public void RemoveContact(Guid contactId, Guid patientId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId && c.PatientId == patientId);
        if (contact != null)
            _contacts.Remove(contact);
    }

    private Patient() : base() { }

}
