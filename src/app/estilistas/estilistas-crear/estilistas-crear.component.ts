import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ServicioService } from '../../servicios/servicios/servicio.service';
import { Servicio } from '../../servicios/modelos/servicio';
import Swal from 'sweetalert2';
import {EstilistasService} from "../servicios/estilistas.service";

@Component({
  selector: 'app-estilistas-crear',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './estilistas-crear.component.html',
  styleUrls: ['./estilistas-crear.component.css']
})
export class EstilistasCrearComponent implements OnInit {

  // Modelo del formulario
  estilista = {
    nombreUsuario: '',
    email: '',
    telefono: '',
    password: '',
    nombreCompleto: '', // <--- AÑADIDO
    servicios: [] as number[] // ID's de servicios seleccionados
  };

  // Lista de servicios disponibles
  serviciosDisponibles: Servicio[] = [];

  // Preview de imagen
  preview: string | null = null;
  imagenSeleccionada: File | null = null;

  constructor(
    private estilistaService: EstilistasService,
    private servicioService: ServicioService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarServicios();
  }

  // Cargar servicios disponibles
  cargarServicios(): void {
    this.servicioService.getServicios().subscribe({
      next: (servicios) => {
        this.serviciosDisponibles = servicios.filter(s => s.disponible);
      },
      error: (err) => {
        console.error('Error al cargar servicios:', err);
        Swal.fire('Error', 'No se pudieron cargar los servicios', 'error');
      }
    });
  }

  // Abrir selector de archivo
  seleccionarImagen(): void {
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    fileInput?.click();
  }

  // Manejar selección de imagen
  onImagenSeleccionada(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // Validar tipo de archivo
      if (!file.type.startsWith('image/')) {
        Swal.fire('Error', 'Por favor selecciona un archivo de imagen válido', 'error');
        return;
      }

      // Validar tamaño (máximo 5MB)
      if (file.size > 5 * 1024 * 1024) {
        Swal.fire('Error', 'La imagen no debe superar los 5MB', 'error');
        return;
      }

      this.imagenSeleccionada = file;

      // Crear preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.preview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  // Crear estilista
  crear(form: NgForm): void {
    if (form.invalid) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos incompletos',
        text: 'Por favor, complete todos los campos obligatorios.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    // Validar que se hayan seleccionado servicios
    if (this.estilista.servicios.length === 0) {
      Swal.fire({
        icon: 'warning',
        title: 'Servicios requeridos',
        text: 'Debe seleccionar al menos un servicio para el estilista.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    // Crear FormData
    const formData = new FormData();
    formData.append('username', this.estilista.nombreUsuario);
    formData.append('email', this.estilista.email);
    formData.append('password', this.estilista.password);
    formData.append('nombreCompleto', this.estilista.nombreCompleto || this.estilista.nombreUsuario);
    formData.append('telefono', this.estilista.telefono);

    // Agregar servicios
    this.estilista.servicios.forEach(servicioId => {
      formData.append('serviciosIds', servicioId.toString());
    });

    // Agregar imagen si existe
    if (this.imagenSeleccionada) {
      formData.append('imagen', this.imagenSeleccionada);
    }

    // Enviar al backend
    this.estilistaService.crearEstilista(formData).subscribe({
      next: (response) => {
        Swal.fire({
          icon: 'success',
          title: 'Estilista Creado',
          text: `El estilista "${response.nombreCompleto}" se creó exitosamente.`,
          timer: 2000,
          showConfirmButton: false
        });
        this.router.navigate(['/admin/estilistas']);
      },
      error: (err) => {
        console.error('Error al crear estilista:', err);
      }
    });
  }

  // Cancelar y volver
  cancelar(): void {
    this.router.navigate(['/admin/estilistas']);
  }

  // En estilistas-crear.component.ts

  onServicioChange(servicioId: number, event: Event): void {
    const checkbox = event.target as HTMLInputElement;

    if (checkbox.checked) {
      // Agregar servicio si no existe
      if (!this.estilista.servicios.includes(servicioId)) {
        this.estilista.servicios.push(servicioId);
      }
    } else {
      // Remover servicio
      const index = this.estilista.servicios.indexOf(servicioId);
      if (index > -1) {
        this.estilista.servicios.splice(index, 1);
      }
    }
  }
}
