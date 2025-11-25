namespace Peluqueria.Application.Exceptions
{
    // 1. Cuando algo no se encuentra (404)
    public class EntidadNoExisteException : PeluqueriaException
    {
        // Opción A: Solo Código (Usa el mensaje por defecto del CodigoError)
        public EntidadNoExisteException(CodigoError codigo)
            : base(codigo) { }

        // Opción B: Solo Mensaje (Usa el código genérico ENTIDAD_NO_ENCONTRADA)
        public EntidadNoExisteException(string mensaje)
            : base(CodigoError.ENTIDAD_NO_ENCONTRADA, mensaje) { }

        // Opción C: Código + Mensaje Personalizado
        public EntidadNoExisteException(CodigoError codigo, string mensaje)
            : base(codigo, mensaje) { }
    }

    // 2. Cuando algo ya existe (409 Conflict)
    public class EntidadYaExisteException : PeluqueriaException
    {
        public EntidadYaExisteException(CodigoError codigo)
            : base(codigo) { }

        public EntidadYaExisteException(string mensaje)
            : base(CodigoError.ENTIDAD_YA_EXISTE, mensaje) { }

        public EntidadYaExisteException(CodigoError codigo, string mensaje)
            : base(codigo, mensaje) { }
    }

    // 3. Reglas de Negocio (400 Bad Request)
    public class ReglaNegocioException : PeluqueriaException
    {
        // Opción A: Solo Código (Ej: PRECIO_INVALIDO)
        public ReglaNegocioException(CodigoError codigo)
            : base(codigo) { }

        // Opción B: Solo Mensaje (Usa ERROR_GENERICO)
        public ReglaNegocioException(string mensaje)
            : base(CodigoError.ERROR_GENERICO, mensaje) { }

        // Opción C: Código + Mensaje Personalizado (¡ESTE ERA EL QUE FALTABA!)
        // Ejemplo: new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "Duración máx 480 min")
        public ReglaNegocioException(CodigoError codigo, string mensaje)
            : base(codigo, mensaje) { }
    }
}