import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import Swal from 'sweetalert2';
import { CategoriaService } from '../servicios/categoria.service';
import { Categoria } from '../modelos/categoria';

@Component({
  selector: 'app-categorias-editar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categorias-editar.component.html',
  styleUrls: ['./categorias-editar.component.css']
})
export class CategoriasEditarComponent implements OnInit {

  categoria: Categoria = {
    id: 0,
    nombre: '',
    estaActiva: true
  };

  constructor(
    private categoriaService: CategoriaService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!id) {
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: 'ID de categoría inválido.',
        confirmButtonText: 'Cerrar'
      });
      this.router.navigate(['/admin/categorias']);
      return;
    }

    this.cargarCategoria(id);
  }

  cargarCategoria(id: number): void {
    this.categoriaService.getCategoriaById(id).subscribe({
      next: (categoria) => {
        this.categoria = categoria;
      },
      error: (err) => {
        console.error('Error al cargar categoría:', err);
        this.router.navigate(['/admin/categorias']);
      }
    });
  }

  actualizar(form: NgForm): void {
    if (form.invalid) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos incompletos',
        text: 'Por favor, complete todos los campos correctamente.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    const categoriaActualizada = {
      nombre: this.categoria.nombre,
      estaActiva: this.categoria.estaActiva
    };

    this.categoriaService.actualizarCategoria(this.categoria.id, categoriaActualizada).subscribe({
      next: (response) => {
        Swal.fire({
          icon: 'success',
          title: 'Categoría Actualizada',
          text: `La categoría "${response.nombre}" se actualizó exitosamente.`,
          timer: 2000,
          showConfirmButton: false
        });
        this.router.navigate(['/admin/categorias']);
      },
      error: (err) => {
        console.error('Error al actualizar categoría:', err);
      }
    });
  }

  cancelar(): void {
    this.router.navigate(['/admin/categorias']);
  }
}
