using Peluqueria.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Peluqueria.Infrastructure.Service
{
    /// <summary>
    /// Servicio de infraestructura para el almacenamiento físico de archivos.
    /// </summary>
    /// <remarks>
    /// Actualmente implementa almacenamiento local en la carpeta <c>wwwroot</c> del servidor web.
    /// En el futuro, esta implementación podría cambiarse por Azure Blob Storage o AWS S3 sin afectar al resto del sistema.
    /// </remarks>
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Guarda un archivo recibido vía HTTP en una subcarpeta específica del servidor.
        /// </summary>
        /// <param name="file">El archivo enviado en el formulario (IFormFile).</param>
        /// <param name="subfolder">La carpeta destino dentro de wwwroot (ej: "images/estilistas").</param>
        /// <returns>
        /// El nombre único generado para el archivo (GUID + Extensión).
        /// </returns>
        public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            var folderPath = Path.Combine(_env.WebRootPath, subfolder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generar nombre único para evitar colisiones si dos usuarios suben "foto.jpg"
            var originalFilename = file.FileName;
            var extension = Path.GetExtension(originalFilename);
            var uniqueFilename = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFilename);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return uniqueFilename;
        }
    }
}