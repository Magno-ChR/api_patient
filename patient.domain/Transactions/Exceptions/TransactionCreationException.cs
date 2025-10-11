using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Transactions.Exceptions;

public class TransactionCreationException :
    Exception
{
    public TransactionCreationException(string reason)
        : base("The transaction cannot be created because: " + reason)
    {
    }
}
