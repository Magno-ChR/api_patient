using patient.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Entities.Backgrounds
{
    public class Background : Entity
    {
        public Guid HistoryId { get; private set; }
        public DateTime RegisterDate { get; private set; }
        public string Description { get; private set; }

        public Background(Guid id, Guid historyId, DateTime registerDate, string description)
            : base(id)
        {
            if (historyId == Guid.Empty)
                throw new ArgumentException("El ID de la historia no puede estar vacío", nameof(historyId));
            if (registerDate > DateTime.UtcNow)
                throw new ArgumentException("La fecha de registro no puede ser en el futuro", nameof(registerDate));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción no puede estar vacía", nameof(description));
            HistoryId = historyId;
            RegisterDate = registerDate;
            Description = description;
        }   

        private Background() : base() { }
    }
}
