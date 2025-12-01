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
    nombreUsuario: '',
    email: '',
    telefono: '',
    password: '',
    servicios: [] as string[]
  };

  preview: string | null = null;
  imagenSeleccionada: File | null = null;

  // Servicios disponibles
  serviciosDisponibles: string[] = [
    'Corte', 'Color', 'Peinado', 'Manicure', 'Pedicure'
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

  this.estilistasService.crear(this.estilista).subscribe({
    next: () => {
      Swal.fire("Éxito", "Estilista creado correctamente", "success");
      this.router.navigate(['/admin/estilistas']);
    },
    error: err => {
      if (err.status === 409) {
        Swal.fire("Error", "El usuario o email ya existen.", "error");
      } else {
        Swal.fire("Error", "No se pudo crear el estilista.", "error");
      }
    }
  });

  return; // ← ESTA LÍNEA ELIMINA TU ERROR
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
