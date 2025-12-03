namespace Peluqueria.Application.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta acceder o manipular un recurso que no existe en la base de datos.
    /// </summary>
    /// <remarks>
    /// El Middleware capturará esta excepción y retornará un código HTTP **404 Not Found**.
    /// </remarks>
    public class EntidadNoExisteException : PeluqueriaException
    {
        /// <summary>
        /// Inicializa la excepción utilizando un <see cref="CodigoError"/> específico.
        /// El mensaje será el definido por defecto en dicho código.
        /// </summary>
        /// <param name="codigo">El código de error del dominio (ej. SERVICIO_NO_ENCONTRADO).</param>
        public EntidadNoExisteException(CodigoError codigo)
            : base(codigo) { }

        /// <summary>
        /// Inicializa la excepción con un mensaje personalizado.
        /// Utiliza internamente el código genérico <see cref="CodigoError.ENTIDAD_NO_ENCONTRADA"/>.
        /// </summary>
        /// <param name="mensaje">Descripción detallada del recurso que falta.</param>
        public EntidadNoExisteException(string mensaje)
            : base(CodigoError.ENTIDAD_NO_ENCONTRADA, mensaje) { }

        /// <summary>
        /// Inicializa la excepción con un código específico y un mensaje personalizado.
        /// Útil para dar detalles extra sobre un error específico.
        /// </summary>
        /// <param name="codigo">El código de error del dominio.</param>
        /// <param name="mensaje">Descripción detallada del error.</param>
        public EntidadNoExisteException(CodigoError codigo, string mensaje)
            : base(codigo, mensaje) { }
    }

    /// <summary>
    /// Excepción lanzada cuando se intenta crear un recurso que viola una restricción de unicidad (duplicado).
    /// </summary>
    /// <remarks>
    /// Ejemplos: Crear un usuario con email repetido o una categoría con el mismo nombre.
    /// El Middleware retornará un código HTTP **409 Conflict**.
    /// </remarks>
    public class EntidadYaExisteException : PeluqueriaException
    {
        /// <summary>
        /// Inicializa la excepción utilizando un <see cref="CodigoError"/> específico.
        /// </summary>
        /// <param name="codigo">El código de error de duplicidad (ej. CATEGORIA_YA_EXISTE).</param>
        public EntidadYaExisteException(CodigoError codigo)
            : base(codigo) { }

        /// <summary>
        /// Inicializa la excepción con un mensaje personalizado.
        /// Utiliza internamente el código genérico <see cref="CodigoError.ENTIDAD_YA_EXISTE"/>.
        /// </summary>
        /// <param name="mensaje">Descripción del conflicto encontrado.</param>
        public EntidadYaExisteException(string mensaje)
            : base(CodigoError.ENTIDAD_YA_EXISTE, mensaje) { }

        /// <summary>
        /// Inicializa la excepción con un código específico y un mensaje personalizado.
        /// </summary>
        /// <param name="codigo">El código de error del dominio.</param>
        /// <param name="mensaje">Descripción detallada del conflicto.</param>
        public EntidadYaExisteException(CodigoError codigo, string mensaje)
            : base(codigo, mensaje) { }
    }

    /// <summary>
    /// Excepción lanzada cuando se viola una regla lógica del negocio o una validación operativa.
    /// </summary>
    /// <remarks>
    /// Ejemplos: "La fecha final es menor a la inicial", "El precio no puede ser negativo".
    /// El Middleware retornará un código HTTP **400 Bad Request**.
    /// </remarks>
    public class ReglaNegocioException : PeluqueriaException
    {
        /// <summary>
        /// Inicializa la excepción utilizando un código de negocio específico.
        /// </summary>
        /// <param name="codigo">El código de error de negocio (ej. PRECIO_INVALIDO).</param>
        public ReglaNegocioException(CodigoError codigo)
            : base(codigo) { }

        /// <summary>
        /// Inicializa la excepción con un mensaje personalizado.
        /// Utiliza internamente el código genérico <see cref="CodigoError.ERROR_GENERICO"/>.
        /// </summary>
        /// <param name="mensaje">Descripción de la regla violada.</param>
        public ReglaNegocioException(string mensaje)
            : base(CodigoError.ERROR_GENERICO, mensaje) { }

        /// <summary>
        /// Inicializa la excepción con un código específico y un mensaje personalizado.
        /// Es la opción recomendada para validaciones detalladas.
        /// </summary>
        /// <example>
        /// new ReglaNegocioException(CodigoError.FORMATO_INVALIDO, "Duración máx 480 min");
        /// </example>
        /// <param name="codigo">El código de error del dominio.</param>
        /// <param name="mensaje">Descripción detallada de la validación fallida.</param>
        public ReglaNegocioException(CodigoError codigo, string mensaje)
            : base(codigo, mensaje) { }
    }
}