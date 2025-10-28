﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Peluqueria.Domain.Entities
{
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public double Precio { get; set; }
        public string Imagen { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Disponible { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
    }
}