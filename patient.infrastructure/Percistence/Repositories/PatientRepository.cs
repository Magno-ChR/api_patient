﻿using Microsoft.EntityFrameworkCore;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Patients;
using patient.domain.Entities.Patients.Events;
using patient.infrastructure.Percistence.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.Repositories;

//No se necesario que sea publico solo se usará aqui
internal class PatientRepository : IPatientRepository
{
    private readonly DomainDbContext context;

    public PatientRepository(DomainDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(Patient entity)
    {
       await context.Patients.AddAsync(entity);
    }


    public async Task<Patient?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Patients
                .AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.Patients
                .FindAsync(id);
        }
    }

    public Task UpdateAsync(Patient entity)
    {
        var added = entity.DomainEvents.Where(x => x is ContactCreateEvent)
            .Select(e => (ContactCreateEvent)e).ToList();

        foreach (var domainEvent in added)
        {
            var itemToAdd = entity.Contacts.First(c => c.Id == domainEvent.ContactId);
            context.Contacts.Add(itemToAdd);
        }

        context.Patients.Update(entity);

        return Task.CompletedTask;
    } 

}
