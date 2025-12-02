import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { CategoriaService } from '../servicios/categoria.service';

@Component({
  selector: 'app-categorias-crear',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categorias-crear.component.html',
  styleUrls: ['./categorias-crear.component.css']
})
export class CategoriasCrearComponent {

  categoria = {
    nombre: ''
  };

  constructor(
    private categoriaService: CategoriaService,
    private router: Router
  ) {}

  crear(form: NgForm): void {
    if (form.invalid) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos incompletos',
        text: 'Por favor, complete todos los campos correctamente.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    this.categoriaService.crearCategoria(this.categoria).subscribe({
      next: (response) => {
        Swal.fire({
          icon: 'success',
          title: 'Categoría Creada',
          text: `La categoría "${response.nombre}" se creó exitosamente.`,
          timer: 2000,
          showConfirmButton: false
        });
        this.router.navigate(['/admin/categorias']);
      },
      error: (err) => {
        console.error('Error al crear categoría:', err);
      }
    });
  }

  cancelar(): void {
    this.router.navigate(['/admin/categorias']);
  }
}
