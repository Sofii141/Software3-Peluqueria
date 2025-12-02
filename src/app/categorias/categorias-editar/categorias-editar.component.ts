import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { CategoriaService } from '../servicios/categoria.service';
import { Categoria } from '../modelos/categoria';

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
      error: () => alert('Error al cargar categoría')
    });
  }

  actualizar(form: NgForm) {
    if (form.invalid) return;

    this.categoriaService.actualizarCategoria(this.categoria.id, this.categoria)
      .subscribe({
        next: () => this.router.navigate(['/admin/categorias']),
        error: () => alert('Error al actualizar')
      });
  }
  
}
