using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence;

/// <summary>
/// Sirve para no tener que ejecutar la migración desde la consola
/// </summary>
internal interface IDatabase
{
    void Migrate();
}
