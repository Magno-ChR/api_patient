using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Abstractions
{
    public interface IRepository<TEntity> where TEntity : AggregateRoot
    {
        Task<TEntity?> GetByIdAsync(Guid id, bool readOnly = false);
        Task AddAsync(TEntity entity);
    }
}
