using patient.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace patient.domain.Transactions;

public interface ITransactionRepository : IRepository<Transaction>
{

}
