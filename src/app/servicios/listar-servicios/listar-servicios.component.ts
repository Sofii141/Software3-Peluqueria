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

  serviciosFiltrados: Servicio[] = [];
  todosLosServicios: Servicio[] = []; // ← NUEVO: Guardamos todos los servicios
  categorias: Categoria[] = [];
  categoriaActiva: string | number = 'all';
  filtroEstado: 'todos' | 'activos' | 'inactivos' = 'activos'; // ← NUEVO
  public isAdmin: boolean = false;

  constructor(
    private objServicioService: ServicioService,
    private objCategoriaService: CategoriaService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();

    this.objCategoriaService.getCategorias().subscribe(categorias => {
      this.categorias = categorias;
    });

    this.cargarServicios();
  }

  // ← NUEVO: Método centralizado para cargar servicios
  cargarServicios(): void {
    const idParaPeticion = this.categoriaActiva === 'all' ? 0 : Number(this.categoriaActiva);

    this.objServicioService.getServiciosPorCategoria(idParaPeticion).subscribe(
      servicios => {
        this.todosLosServicios = servicios;
        this.aplicarFiltros();
      },
      error => {
        console.error('Error al cargar servicios:', error);
        this.serviciosFiltrados = [];
      }
    );
  }

  // ← NUEVO: Aplicar filtros de categoría y estado
  aplicarFiltros(): void {
    let serviciosFiltrados = [...this.todosLosServicios];

    // Filtrar por estado (activos/inactivos)
    if (this.filtroEstado === 'activos') {
      serviciosFiltrados = serviciosFiltrados.filter(s => s.disponible);
    } else if (this.filtroEstado === 'inactivos') {
      serviciosFiltrados = serviciosFiltrados.filter(s => !s.disponible);
    }

    this.serviciosFiltrados = serviciosFiltrados;
  }

  filtrarServicios(categoriaId: string | number): void {
    this.categoriaActiva = categoriaId;
    this.cargarServicios();
  }

  // ← NUEVO: Cambiar filtro de estado
  cambiarFiltroEstado(estado: 'todos' | 'activos' | 'inactivos'): void {
    this.filtroEstado = estado;
    this.aplicarFiltros();
  }

  editarServicio(id: number): void {
    this.router.navigate(['/servicios/actualizar', id]);
  }

  // ← MODIFICADO: Ahora maneja inactivar Y reactivar
  toggleEstadoServicio(servicio: Servicio): void {
    if (servicio.disponible) {
      // Si está activo, intentamos inactivar
      this.inactivarServicio(servicio);
    } else {
      // Si está inactivo, reactivamos directamente
      this.reactivarServicio(servicio);
    }
  }

  private inactivarServicio(servicio: Servicio): void {
    // Verificamos si tiene reservas futuras
    this.objServicioService.verificarReservasFuturas(servicio.id).subscribe({
      next: (tieneReservas) => {
        if (tieneReservas) {
          Swal.fire({
            title: 'No se puede inactivar',
            html: `
              <p>El servicio <strong>${servicio.nombre}</strong> tiene <strong>citas futuras programadas</strong>.</p>
              <p>Debe cancelar o reasignar las citas antes de inactivarlo.</p>
            `,
            icon: 'error',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#d33'
          });
        } else {
          this.confirmarInactivacion(servicio);
        }
      },
      error: () => {
        Swal.fire({
          title: 'No se pudo verificar las reservas',
          text: 'Por seguridad, no se puede inactivar el servicio en este momento.',
          icon: 'warning',
          confirmButtonText: 'Cerrar'
        });
      }
    });
  }

  private confirmarInactivacion(servicio: Servicio): void {
    Swal.fire({
      title: '¿Inactivar servicio?',
      html: `
        <p>El servicio <strong>${servicio.nombre}</strong> dejará de estar disponible para reservas.</p>
        <p class="text-muted">Podrás reactivarlo en cualquier momento.</p>
      `,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, inactivar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.objServicioService.deleteServicio(servicio.id).subscribe({
          next: () => {
            Swal.fire({
              title: 'Servicio Inactivado',
              text: `${servicio.nombre} ha sido inactivado correctamente.`,
              icon: 'success',
              timer: 2000,
              showConfirmButton: false
            });

            this.cargarServicios();
          },
          error: (err) => {
            console.error('Error al inactivar servicio:', err);
          }
        });
      }
    });
  }

  private reactivarServicio(servicio: Servicio): void {
    Swal.fire({
      title: '¿Reactivar servicio?',
      html: `
        <p>El servicio <strong>${servicio.nombre}</strong> volverá a estar disponible para reservas.</p>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#28a745',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, reactivar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.objServicioService.reactivarServicio(servicio.id).subscribe({
          next: () => {
            Swal.fire({
              title: 'Servicio Reactivado',
              text: `${servicio.nombre} está nuevamente disponible.`,
              icon: 'success',
              timer: 2000,
              showConfirmButton: false
            });

            this.cargarServicios();
          },
          error: (err) => {
            console.error('Error al reactivar servicio:', err);
          }
        });
      }
    });
  }
}
