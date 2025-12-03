import { Component, OnInit } from '@angular/core';
import { CommonModule} from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2';

import { CategoriaService } from '../../categorias/servicios/categoria.service';
import { Categoria } from '../../categorias/modelos/categoria';
import {Servicio, ServicioForm} from "../modelos/servicio";
import {ServiciosService} from "../servicios/servicio.service";

@Component({
  selector: 'app-servicios-editar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './servicios-editar.component.html',
  styleUrls: ['./servicios-editar.component.css']
})
export class ServiciosEditarComponent implements OnInit {

  id!: number;

  servicioForm: ServicioForm = {
    nombre: '',
    descripcion: '',
    precio: 0,
    disponible: true,
    categoriaId: 0
  };

  categorias: Categoria[] = [];

  imagenSeleccionada: File | null = null;
  preview: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private serviciosService: ServiciosService,
    private categoriaService: CategoriaService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));

    this.categoriaService.getCategorias().subscribe({
      next: data => this.categorias = data,
      error: () => Swal.fire('Error', 'No se pudieron cargar las categorías.', 'error')
    });

    this.serviciosService.getServicio(this.id).subscribe({
      next: (s: Servicio) => {
        this.servicioForm = {
          nombre: s.nombre,
          descripcion: s.descripcion,
          precio: s.precio,
          disponible: s.disponible,
          categoriaId: s.categoria?.id
        };

        // Si viene una URL de imagen válida, úsala como preview
        if (s.imagen) {
          this.preview = s.imagen;
        }
      },
      error: () => {
        Swal.fire('Error', 'No se pudo cargar el servicio.', 'error');
        this.router.navigate(['/admin/servicios']);
      }
    });
  }

  seleccionarImagen(): void {
    document.getElementById('fileInput')?.click();
  }

  onImagenSeleccionada(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    this.imagenSeleccionada = file;

    const reader = new FileReader();
    reader.onload = () => this.preview = reader.result as string;
    reader.readAsDataURL(file);
  }

  actualizar(form: NgForm): void {
    if (form.invalid || !this.servicioForm.categoriaId) {
      Swal.fire('Formulario incompleto', 'Verifica los campos obligatorios.', 'warning');
      return;
    }

    this.serviciosService.actualizarServicio(this.id, this.servicioForm, this.imagenSeleccionada).subscribe({
      next: () => {
        Swal.fire('Éxito', 'Servicio actualizado correctamente.', 'success');
        this.router.navigate(['/admin/servicios']);
      },
      error: () => {
        Swal.fire('Error', 'No se pudo actualizar el servicio.', 'error');
      }
    });
  }
}
