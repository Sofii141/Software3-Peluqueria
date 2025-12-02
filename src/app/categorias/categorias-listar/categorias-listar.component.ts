import { Component, OnInit } from '@angular/core';
import { CategoriaService } from '../servicios/categoria.service';
import { Categoria } from '../modelos/categoria';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';
import { NgFor} from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-categorias-listar',
  standalone: true,
  templateUrl: './categorias-listar.component.html',
  styleUrls: ['./categorias-listar.component.css'],
  imports: [NgFor, FormsModule, RouterModule]
})
export class CategoriasListarComponent implements OnInit {

  categorias: Categoria[] = [];
  categoriasFiltradas: Categoria[] = [];
  filtro: string = '';

  constructor(private categoriaService: CategoriaService) {}

  ngOnInit(): void {
    this.cargarCategorias();
  }

  cargarCategorias() {
    this.categoriaService.getCategorias().subscribe({
      next: (data) => {
        this.categorias = data;
        this.categoriasFiltradas = data;
      },
      error: () => {
        Swal.fire("Error", "No se pudieron cargar las categorías.", "error");
      }
    });
  }

  filtrar() {
    const texto = this.filtro.toLowerCase();
    this.categoriasFiltradas = this.categorias.filter(cat =>
      cat.nombre.toLowerCase().includes(texto)
    );
  }

  inactivar(cat: Categoria) {
    Swal.fire({
      title: "¿Inactivar categoría?",
      text: "Esta categoría no estará disponible en el catálogo.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Sí, inactivar"
    }).then(result => {
      if (!result.isConfirmed) return;

      this.categoriaService.inactivarCategoria(cat.id).subscribe({
        next: () => {
          Swal.fire("Listo", "Categoría inactivada.", "success");
          this.cargarCategorias();
        },
        error: () => {
          Swal.fire("Error", "No se pudo inactivar la categoría.", "error");
        }
      });
    });
  }
}
