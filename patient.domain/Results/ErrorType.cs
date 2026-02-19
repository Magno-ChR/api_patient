using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Results;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6
}
