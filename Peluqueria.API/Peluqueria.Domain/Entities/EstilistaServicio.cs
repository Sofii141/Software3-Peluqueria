using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    // Entidad de relación M:N
    public class EstilistaServicio
    {
        public int EstilistaId { get; set; }
        public Estilista Estilista { get; set; } = null!;
        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
    }
}
