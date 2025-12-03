import { Component, OnInit } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import Swal from 'sweetalert2';
import {Servicio} from "../modelos/servicio";
import {ServiciosService} from "../servicios/servicio.service";


@Component({
  selector: 'app-servicios-listar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NgIf],
  templateUrl: './servicios-listar.component.html',
  styleUrls: ['./servicios-listar.component.css']
})
export class ServiciosListarComponent implements OnInit {

  servicios: Servicio[] = [];
  serviciosFiltrados: Servicio[] = [];
  filtro: string = '';

  constructor(private serviciosService: ServiciosService) {}

  ngOnInit(): void {
    this.cargarServicios();
  }

  cargarServicios(): void {
    this.serviciosService.getServicios().subscribe({
      next: data => {
        this.servicios = data;
        this.serviciosFiltrados = data;
      },
      error: () => {
        Swal.fire('Error', 'No se pudieron cargar los servicios.', 'error');
      }
    });
  }

  filtrar(): void {
    const texto = this.filtro.toLowerCase();
    this.serviciosFiltrados = this.servicios.filter(s =>
      s.nombre.toLowerCase().includes(texto) ||
      s.descripcion.toLowerCase().includes(texto)
    );
  }

  eliminar(servicio: Servicio): void {
    Swal.fire({
      title: '¿Eliminar servicio?',
      text: `Se eliminará "${servicio.nombre}". Esta acción no se puede deshacer.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    }).then(result => {
      if (!result.isConfirmed) return;

      this.serviciosService.eliminarServicio(servicio.id).subscribe({
        next: () => {
          Swal.fire('Eliminado', 'El servicio fue eliminado correctamente.', 'success');
          this.cargarServicios();
        },
        error: () => {
          Swal.fire('Error', 'No se pudo eliminar el servicio.', 'error');
        }
      });
    });
  }
}
