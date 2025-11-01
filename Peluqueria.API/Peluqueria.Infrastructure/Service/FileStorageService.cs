using Peluqueria.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

using Microsoft.AspNetCore.Http;

namespace Peluqueria.Infrastructure.Service
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            var folderPath = Path.Combine(_env.WebRootPath, subfolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

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