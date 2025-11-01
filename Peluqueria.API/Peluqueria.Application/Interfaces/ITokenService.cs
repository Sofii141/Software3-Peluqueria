using Peluqueria.Application.Dtos.Account; 

namespace Peluqueria.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUserMinimalDto user, IList<string> roles);
    }
}