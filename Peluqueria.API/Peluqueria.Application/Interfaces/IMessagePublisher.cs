using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IMessagePublisher
    {
        // Publica cualquier evento de cambio de datos maestros
        // Usamos object y generic para que sea flexible a cualquier DTO/modelo de evento
        Task PublishAsync<T>(T message, string routingKey, string exchangeName) where T : class;
    }
}
