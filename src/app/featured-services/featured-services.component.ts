import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { Servicio } from '../servicios/modelos/servicio';
import { ServiciosService } from '../servicios/servicios/servicio.service'; // ✅ nombre corregido
import { Categoria } from '../categorias/modelos/categoria';
import { CategoriaService } from '../categorias/servicios/categoria.service';

@Component({
  selector: 'app-featured-services',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './featured-services.component.html',
  styleUrls: ['./featured-services.component.css']
})
export class FeaturedServicesComponent implements OnInit {

  public serviciosFiltrados: Servicio[] = [];
  public categorias: Categoria[] = [];
  public categoriaActiva: number | 'all' = 'all';

  constructor(
    private servicioService: ServiciosService, // ✅ nombre corregido
    private categoriaService: CategoriaService
  ) {}

  ngOnInit(): void {
    this.categoriaService.getCategorias().subscribe((cats: Categoria[]) => {
      this.categorias = cats;
    });

    this.filtrarServicios('all');
  }

  filtrarServicios(idCategoria: number | 'all'): void {
  this.categoriaActiva = idCategoria;

  if (idCategoria === 'all') {
    // ✅ Llama al endpoint que trae todos los servicios sin filtrar
    this.servicioService.getServicios().subscribe(
      servicios => {
        this.serviciosFiltrados = servicios.filter(s => s.disponible);
      },
      error => {
        console.error('Error al obtener todos los servicios:', error);
        this.serviciosFiltrados = [];
      }
    );
    return;
  }

  //  Si hay una categoría válida, filtra por esa
  this.servicioService.getServiciosPorCategoria(idCategoria).subscribe(
    servicios => {
      this.serviciosFiltrados = servicios.filter(s => s.disponible);
    },
    error => {
      console.error('Error al filtrar servicios desde el backend:', error);
      this.serviciosFiltrados = [];
    }
  );
}

}
