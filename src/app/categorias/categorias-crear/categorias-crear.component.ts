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
    nombre: '',
    estado: true
  };

  constructor(
    private categoriaService: CategoriaService,
    private router: Router
  ) {}

  crear(form: NgForm): void {
    if (form.invalid) {
      Swal.fire('Campos incompletos', 'Revisa el formulario.', 'warning');
      return;
    }

    this.categoriaService.crearCategoria(this.categoria).subscribe({
      next: () => {
        Swal.fire('Éxito', 'Categoría creada correctamente', 'success');
        this.router.navigate(['/admin/categorias']);
      },
      error: (err) => {
        console.error(err);
        Swal.fire('Error', 'No se pudo crear la categoría.', 'error');
      }
    });
  }

  cerrar(): void {
    this.router.navigate(['/admin/categorias']);
  }
}
