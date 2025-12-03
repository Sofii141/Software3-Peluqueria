import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2';

import { CategoriaService } from '../../categorias/servicios/categoria.service';
import { Categoria } from '../../categorias/modelos/categoria';
import { ServicioForm } from '../modelos/servicio';
import { ServiciosService } from '../servicios/servicio.service';

@Component({
  selector: 'app-servicios-crear',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './servicios-crear.component.html',
  styleUrls: ['./servicios-crear.component.css']
})
export class ServiciosCrearComponent implements OnInit {

  servicio: ServicioForm = {
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
    private serviciosService: ServiciosService,
    private categoriaService: CategoriaService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.categoriaService.getCategorias().subscribe({
      next: data => this.categorias = data,
      error: () => Swal.fire('Error', 'No se pudieron cargar las categorías.', 'error')
    });
  }

  onImagenSeleccionada(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0];
    if (!file) return;

    this.imagenSeleccionada = file;

    const reader = new FileReader();
    reader.onload = () => this.preview = reader.result as string;
    reader.readAsDataURL(file);
  }

  seleccionarImagen(): void {
    document.getElementById('fileInput')?.click();
  }

  crear(form: NgForm): void {
    if (form.invalid || !this.servicio.categoriaId) {
      Swal.fire('Formulario incompleto', 'Verifica los campos obligatorios.', 'warning');
      return;
    }

    this.serviciosService.crearServicio(this.servicio, this.imagenSeleccionada).subscribe({
      next: () => {
        Swal.fire('Éxito', 'Servicio creado correctamente.', 'success');
        this.router.navigate(['/admin/servicios']);
      },
      error: () => {
        Swal.fire('Error', 'No se pudo crear el servicio.', 'error');
      }
    });
  }
}
