import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import Swal from 'sweetalert2';
import { EstilistasService } from '../estilistas.service';
import { Estilista } from '../estilista.model';

@Component({
  selector: 'app-estilistas-editar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './estilistas-editar.component.html',
  styleUrls: ['./estilistas-editar.component.css']
})
export class EstilistasEditarComponent implements OnInit {

  estilista!: Estilista;
  serviciosDisponibles: { id: number, nombre: string }[] = [];

  imagenSeleccionada: File | null = null;
  preview: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private estilistasService: EstilistasService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!id) {
      Swal.fire("Error", "ID inválido", "error");
      this.router.navigate(['/admin/estilistas']);
      return;
    }

    this.cargarEstilista(id);
    this.cargarServicios();
  }

  cargarEstilista(id: number) {
    this.estilistasService.obtener(id).subscribe({
      next: (data) => {
        this.estilista = { ...data };

        // Si los servicios vienen como strings, puedes convertirlos a IDs si es necesario
        // this.estilista.servicios = this.convertirServiciosANumeros(data.servicios);

        this.preview = this.estilista.imagenUrl || null;
      },
      error: () => {
        Swal.fire("Error", "No se pudo cargar el estilista.", "error");
        this.router.navigate(['/admin/estilistas']);
      }
    });
  }

  cargarServicios() {
    this.serviciosDisponibles = [
      { id: 1, nombre: "Corte" },
      { id: 2, nombre: "Color" },
      { id: 3, nombre: "Manicure" },
      { id: 4, nombre: "Pedicure" },
      { id: 5, nombre: "Peinados" },
      { id: 6, nombre: "Tratamientos" }
    ];
  }

  onImagenSeleccionada(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    this.imagenSeleccionada = file;

    const reader = new FileReader();
    reader.onload = () => (this.preview = reader.result as string);
    reader.readAsDataURL(file);
  }
  seleccionarImagen() {
    const fileInput = document.getElementById('fileInput') as HTMLElement;
    fileInput.click();
  }
  actualizar(form: NgForm) {
    if (form.invalid) {
      Swal.fire("Formulario incompleto", "Revisa los campos.", "warning");
      return;
    }

    Swal.fire({
      title: "¿Guardar cambios?",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Guardar",
      cancelButtonText: "Cancelar"
    }).then(res => {
      if (!res.isConfirmed) return;

      const formData = new FormData();

      formData.append('Username', this.estilista.username);
      formData.append('NombreCompleto', this.estilista.nombreCompleto || '');

      formData.append('Email', this.estilista.email);
      formData.append('Telefono', this.estilista.telefono);
      formData.append('Estado', String(this.estilista.estado));

      // Si quieres actualizar contraseña, puedes agregarla opcionalmente
      if (this.estilista.password?.trim()) {
        formData.append('Password', this.estilista.password);
      }

      this.estilista.servicios.forEach(servicioId => {
        formData.append('ServiciosIds', servicioId.toString());
      });

      if (this.imagenSeleccionada) {
        formData.append('Imagen', this.imagenSeleccionada);
      }

      this.estilistasService.actualizar(this.estilista.id!, formData, this.imagenSeleccionada)
        .subscribe({
          next: () => {
            Swal.fire("Actualizado", "El estilista fue modificado.", "success");
            this.router.navigate(['/admin/estilistas']);
          },
          error: (err) => {
            Swal.fire(
              "Error",
              err.status === 409
                ? "El nombre de usuario o email ya existen."
                : "No se pudo actualizar el estilista.",
              "error"
            );
          }
        });

    });
  }
}
