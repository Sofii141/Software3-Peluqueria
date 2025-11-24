using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Domain.Entities
{
    public class ParametroSistema
    {
        public int Id { get; set; } = 1;
        public int BufferMinutos { get; set; } = 5; // RN-BUFFER
        public int ToleranciaLlegadaMinutos { get; set; } = 10; // RN-TOLERANCIA
        public int DuracionMinimaServicioMinutos { get; set; } = 45; // RN-DURACIÓN-MIN
    }
}
