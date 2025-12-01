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

  estilista!: Estilista;          // Estilista cargado desde backend
  serviciosDisponibles: string[] = [];
  servicioSeleccionado: string = "";

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

  /** Obtener datos del estilista por ID */
  cargarEstilista(id: number) {
    this.estilistasService.listar().subscribe({
      next: (lista: Estilista[]) => {
        const encontrado = lista.find(e => e.id === id);

        if (!encontrado) {
          Swal.fire("No encontrado", "El estilista no existe.", "error");
          this.router.navigate(['/admin/estilistas']);
          return;
        }

        this.estilista = { ...encontrado };
      },
      error: () => {
        Swal.fire("Error", "No se pudo cargar el estilista.", "error");
      }
    });
  }

  /** Servicios que ya existen en el sistema */
  cargarServicios() {
    this.serviciosDisponibles = [
      "Corte",
      "Color",
      "Manicure",
      "Pedicure",
      "Tratamientos",
      "Peinados",
      "Depilación"
    ];
  }

  /** Actualizar estilista */
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

        const payload: any = {
            nombre: this.estilista.nombre,
            nombreUsuario: this.estilista.nombreUsuario,
            email: this.estilista.email,
            telefono: this.estilista.telefono,
            estado: this.estilista.estado,
            servicios: this.estilista.servicios
        };

    
      this.estilistasService.actualizar(this.estilista.id, payload).subscribe({
        next: () => {
          Swal.fire("Actualizado", "El estilista fue modificado.", "success");
          this.router.navigate(['/admin/estilistas']);
        },
        error: (err) => {
          console.error(err);

          Swal.fire(
            "Error",
            err.status === 409 ?
              "El nombre de usuario o email ya existen." :
              "No se pudo actualizar el estilista.",
            "error"
          );
        }
      });

    });
  }

}
