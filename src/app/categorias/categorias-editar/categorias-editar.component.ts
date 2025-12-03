import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { CategoriaService } from '../servicios/categoria.service';
import { Categoria } from '../modelos/categoria';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-categorias-editar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './categorias-editar.component.html',
  styleUrls: ['./categorias-editar.component.css']
})
export class CategoriasEditarComponent implements OnInit {

  categoria: Categoria = { id: 0, nombre: '', activa: true };

  constructor(
    private route: ActivatedRoute,
    private categoriaService: CategoriaService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.params['id']);

    this.categoriaService.getCategoria(id).subscribe({
      next: (data) => this.categoria = data,
      error: (err) => {
        console.error(err);
        Swal.fire('Error', 'No se pudo cargar la categoría.', 'error');
        this.router.navigate(['/admin/categorias']);
      }
    });
  }

  actualizar(form: NgForm): void {
    if (form.invalid) {
      Swal.fire('Formulario inválido', 'Revisa los campos antes de continuar.', 'warning');
      return;
    }

    this.categoriaService.actualizarCategoria(this.categoria.id, this.categoria).subscribe({
      next: () => {
        Swal.fire('Actualizado', 'Categoría actualizada correctamente.', 'success');
        this.router.navigate(['/admin/categorias']);
      },
      error: (err) => {
        console.error(err);
        Swal.fire('Error', 'No se pudo actualizar la categoría.', 'error');
      }
    });
  }
}
