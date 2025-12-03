import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { EstilistasService } from '../estilistas.service';

@Component({
  selector: 'app-estilistas-crear',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './estilistas-crear.component.html',
  styleUrls: ['./estilistas-crear.component.css']
})
export class EstilistasCrearComponent {

  estilista = {
    username: '',
    nombreCompleto: '',
    email: '',
    telefono: '',
    password: '',
    servicios: [] as number[]
  };

  preview: string | null = null;
  imagenSeleccionada: File | null = null;

  serviciosDisponibles = [
    { id: 1, nombre: 'Corte' },
    { id: 2, nombre: 'Color' },
    { id: 3, nombre: 'Peinado' },
    { id: 4, nombre: 'Manicure' },
    { id: 5, nombre: 'Pedicure' }
  ];

  constructor(
    private estilistasService: EstilistasService,
    private router: Router
  ) {}

  crear(form: NgForm) {

    if (form.invalid) {
      Swal.fire("Campos incompletos", "Revisa el formulario.", "warning");
      return;
    }

    if (this.estilista.servicios.length === 0) {
      Swal.fire("Servicios requeridos", "Debe seleccionar al menos un servicio.", "warning");
      return;
    }

    // 👉 El servicio espera datos sueltos, NO un FormData
    this.estilistasService.crear(this.estilista, this.imagenSeleccionada).subscribe({
      next: () => {
        Swal.fire("Éxito", "Estilista creado correctamente", "success");
        this.router.navigate(['/admin/estilistas']);
      },
      error: err => {
        console.log(err);
        Swal.fire("Error", "No se pudo crear el estilista", "error");
      }
    });
  }

  seleccionarImagen() {
    document.getElementById('fileInput')?.click();
  }

  onImagenSeleccionada(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    this.imagenSeleccionada = file;

    const reader = new FileReader();
    reader.onload = () => this.preview = reader.result as string;
    reader.readAsDataURL(file);
  }
}
