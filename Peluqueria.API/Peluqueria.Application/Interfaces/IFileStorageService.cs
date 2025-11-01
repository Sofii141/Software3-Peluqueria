using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Peluqueria.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subfolder);
        // void DeleteFile(string fileName, string subfolder);
    }
}