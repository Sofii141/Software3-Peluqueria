using System.Collections.Generic; 
using Peluqueria.Domain.Entities;

namespace Peluqueria.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);

        string CreateToken(AppUser user, IList<string> roles);
    }
}