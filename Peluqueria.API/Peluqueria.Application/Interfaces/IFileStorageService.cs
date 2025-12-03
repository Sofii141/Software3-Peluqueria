using Microsoft.AspNetCore.Http;

namespace Peluqueria.Application.Interfaces
{
    /// <summary>
    /// Servicio para el manejo de almacenamiento de archivos estáticos (imágenes).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un archivo físico en el servidor (wwwroot).
        /// </summary>
        /// <param name="file">El archivo recibido del formulario.</param>
        /// <param name="subfolder">Carpeta destino (ej: "images/estilistas").</param>
        /// <returns>El nombre único generado para el archivo.</returns>
        Task<string> SaveFileAsync(IFormFile file, string subfolder);
    }
}