import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CategoriaService } from '../servicios/categoria.service';
import { Categoria } from '../modelos/categoria';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-categorias-listar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './categorias-listar.component.html',
  styleUrls: ['./categorias-listar.component.css']
})
export class CategoriasListarComponent implements OnInit {

  categorias: Categoria[] = [];
  categoriasFiltradas: Categoria[] = [];
  filtroEstado: 'todos' | 'activos' | 'inactivos' = 'activos';

  constructor(
    private categoriaService: CategoriaService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarCategorias();
  }

  cargarCategorias(): void {
    this.categoriaService.getCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias;
        this.aplicarFiltro();
      },
      error: (err) => {
        console.error('Error al cargar categorías:', err);
      }
    });
  }

  aplicarFiltro(): void {
    if (this.filtroEstado === 'activos') {
      this.categoriasFiltradas = this.categorias.filter(c => c.estaActiva);
    } else if (this.filtroEstado === 'inactivos') {
      this.categoriasFiltradas = this.categorias.filter(c => !c.estaActiva);
    } else {
      this.categoriasFiltradas = [...this.categorias];
    }
  }

  cambiarFiltro(estado: 'todos' | 'activos' | 'inactivos'): void {
    this.filtroEstado = estado;
    this.aplicarFiltro();
  }

  editarCategoria(id: number): void {
    this.router.navigate(['/admin/categorias/editar', id]);
  }

  toggleEstadoCategoria(categoria: Categoria): void {
    if (categoria.estaActiva) {
      this.inactivarCategoria(categoria);
    } else {
      this.reactivarCategoria(categoria);
    }
  }

  private inactivarCategoria(categoria: Categoria): void {
    // Verificar si tiene servicios con reservas futuras
    this.categoriaService.verificarReservasFuturas(categoria.id).subscribe({
      next: (tieneReservas) => {
        if (tieneReservas) {
          Swal.fire({
            title: 'No se puede inactivar',
            html: `
              <p>La categoría <strong>${categoria.nombre}</strong> tiene servicios con <strong>citas futuras programadas</strong>.</p>
              <p>Debe cancelar o reasignar las citas antes de inactivarla.</p>
            `,
            icon: 'error',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#d33'
          });
        } else {
          this.confirmarInactivacion(categoria);
        }
      },
      error: () => {
        Swal.fire({
          title: 'No se pudo verificar las reservas',
          text: 'Por seguridad, no se puede inactivar la categoría en este momento.',
          icon: 'warning',
          confirmButtonText: 'Cerrar'
        });
      }
    });
  }

  private confirmarInactivacion(categoria: Categoria): void {
    Swal.fire({
      title: '¿Inactivar categoría?',
      html: `
        <p>La categoría <strong>${categoria.nombre}</strong> dejará de estar disponible.</p>
        <p class="text-muted">Podrás reactivarla en cualquier momento.</p>
      `,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, inactivar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.categoriaService.inactivarCategoria(categoria.id).subscribe({
          next: () => {
            Swal.fire({
              title: 'Categoría Inactivada',
              text: `${categoria.nombre} ha sido inactivada correctamente.`,
              icon: 'success',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarCategorias();
          },
          error: (err) => {
            console.error('Error al inactivar categoría:', err);
          }
        });
      }
    });
  }

  private reactivarCategoria(categoria: Categoria): void {
    Swal.fire({
      title: '¿Reactivar categoría?',
      html: `
        <p>La categoría <strong>${categoria.nombre}</strong> volverá a estar disponible.</p>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#28a745',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, reactivar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        const categoriaActualizada = {
          nombre: categoria.nombre,
          estaActiva: true
        };

        this.categoriaService.actualizarCategoria(categoria.id, categoriaActualizada).subscribe({
          next: () => {
            Swal.fire({
              title: 'Categoría Reactivada',
              text: `${categoria.nombre} está nuevamente disponible.`,
              icon: 'success',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarCategorias();
          },
          error: (err) => {
            console.error('Error al reactivar categoría:', err);
          }
        });
      }
    });
  }
}
