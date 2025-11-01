using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Peluqueria.Application.Dtos.Account
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty; 
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}