import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Servicio } from '../servicios/modelos/servicio';
import { ServicioService } from '../servicios/servicios/servicio.service';
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
    private servicioService: ServicioService,
    private categoriaService: CategoriaService
  ) { }

  ngOnInit(): void {
    this.categoriaService.getCategorias().subscribe(cats => {
      this.categorias = cats;
    });

    this.filtrarServicios('all');
  }

  filtrarServicios(idCategoria: number | 'all'): void {
    this.categoriaActiva = idCategoria;
    
    // Convertimos 'all' a 0 para que el ServicioService lo maneje correctamente
    const idParaLaPeticion = idCategoria === 'all' ? 0 : idCategoria;

    this.servicioService.getServiciosPorCategoria(idParaLaPeticion).subscribe(
      servicios => {
        // Adicionalmente, filtramos para mostrar solo los que están 'disponibles'
        this.serviciosFiltrados = servicios.filter(s => s.disponible);
      },
      error => {
        console.error('Error al filtrar servicios desde el backend:', error);
        this.serviciosFiltrados = []; // En caso de error, la lista queda vacía
      }
    );
  }
}