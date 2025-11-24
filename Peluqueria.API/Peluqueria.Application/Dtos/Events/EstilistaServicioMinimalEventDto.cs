using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Events
{
    public class EstilistaServicioMinimalEventDto
    {
        public int ServicioId { get; set; }
        public int DuracionMinutos { get; set; }
    }
}
