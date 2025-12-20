using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.test.ContractTest.ContractDTOs;

public class ApiErrorResponseContractDto
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; set; }
    public ErrorContractDto Error { get; set; } = null!;
}

public class ErrorContractDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StructuredMessage { get; set; } = string.Empty;
    public int Type { get; set; }
}