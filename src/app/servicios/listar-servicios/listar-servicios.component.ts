import { Component, OnInit } from '@angular/core';
import { Servicio } from '../modelos/servicio';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ServicioService } from '../servicios/servicio.service';
import { HttpClientModule } from '@angular/common/http';
import Swal from 'sweetalert2';
import { SweetAlert2Module } from '@sweetalert2/ngx-sweetalert2';
import { Categoria } from '../../categorias/modelos/categoria';
import { CategoriaService } from '../../categorias/servicios/categoria.service';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-listar-servicios',
  standalone: true,
  imports: [CommonModule, RouterLink, HttpClientModule, SweetAlert2Module],
  templateUrl: './listar-servicios.component.html',
  styleUrls: ['./listar-servicios.component.css']
})
export class ListarServiciosComponent implements OnInit {

  // --- PROPIEDADES SIMPLIFICADAS ---
  // Ya no necesitamos un array para 'todos los servicios', solo el que se muestra
  serviciosFiltrados: Servicio[] = [];
  categorias: Categoria[] = [];
  categoriaActiva: string | number = 'all';
  public isAdmin: boolean = false;

  constructor(
    private objServicioService: ServicioService,
    private objCategoriaService: CategoriaService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Verificamos el rol del usuario al iniciar
    this.isAdmin = this.authService.isAdmin();
    
    // Obtenemos las categorías para los botones de filtro
    this.objCategoriaService.getCategorias().subscribe(categorias => {
      this.categorias = categorias;
    });

    // --- LÓGICA MEJORADA ---
    // Hacemos la carga inicial de los servicios llamando a nuestra nueva función de filtro
    this.filtrarServicios('all');
  }

  // --- FUNCIÓN DE FILTRADO OPTIMIZADA ---
  // Ahora llama al backend para obtener solo los servicios necesarios
  filtrarServicios(categoriaId: string | number): void {
    this.categoriaActiva = categoriaId;

    // Convertimos 'all' a 0 para que el servicio lo entienda
    const idParaPeticion = categoriaId === 'all' ? 0 : Number(categoriaId);

    this.objServicioService.getServiciosPorCategoria(idParaPeticion).subscribe(
      servicios => {
        this.serviciosFiltrados = servicios;
      },
      error => {
        console.error('Error al cargar servicios por categoría:', error);
        // En caso de error, mostramos una lista vacía para que el usuario vea el mensaje
        this.serviciosFiltrados = [];
      }
    );
  }

  editarServicio(id: number): void {
    this.router.navigate(['/servicios/actualizar', id]);
  }

  // --- FUNCIÓN DE ELIMINACIÓN MEJORADA ---
  eliminarServicio(id: number): void {
    Swal.fire({
      title: '¿Desea eliminar el servicio?',
      text: "La eliminación no se puede revertir",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Confirmar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.objServicioService.deleteServicio(id).subscribe(() => {
          Swal.fire('Eliminado', 'El servicio ha sido eliminado.', 'success');
          
          // En lugar de filtrar arrays manualmente, simplemente volvemos a cargar
          // los datos del filtro que ya estaba activo. Es más limpio y seguro.
          this.filtrarServicios(this.categoriaActiva);
        });
      }
    });
  }
}