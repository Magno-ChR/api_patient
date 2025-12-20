using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.test.ContractTest.ContractDTOs;

public class GetPatientsResponse
{
    public PatientsPageContractDto Value { get; set; } = null!;
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; set; }
    public ErrorContractDto Error { get; set; } = null!;
}

public class PatientsPageContractDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<PatientContractDto> Items { get; set; } = [];
}

public class PatientContractDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int BloodType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

