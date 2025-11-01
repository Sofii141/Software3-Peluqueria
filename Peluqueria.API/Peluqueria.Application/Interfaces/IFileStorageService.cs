using Microsoft.AspNetCore.Http;

namespace Peluqueria.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subfolder);
        // void DeleteFile(string fileName, string subfolder);
    }
}