import { Component, OnInit } from '@angular/core';
import { NgFor, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EstilistasService } from '../estilistas.service';
import { Estilista } from '../estilista.model';
import Swal from 'sweetalert2';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-estilistas-listar',
  standalone: true,
  imports: [NgFor, RouterModule, CommonModule],
  templateUrl: './estilistas-listar.component.html',
  styleUrls: ['./estilistas-listar.component.css'],
})
export class EstilistasListarComponent implements OnInit {

  estilistas: Estilista[] = [];
  estilistasFiltrados: Estilista[] = [];
  serviciosDisponibles: string[] = [];
  servicioSeleccionado: string = 'Todos';

  constructor(private estilistasService: EstilistasService) {}

  ngOnInit(): void {
    this.cargarEstilistas();
  }

  cargarEstilistas() {
    this.estilistasService.listar().subscribe({
      next: (data: Estilista[]) => {

        this.estilistas = data;
        this.estilistasFiltrados = data;

        // Obtener lista de servicios únicos
        const setServicios = new Set<string>();

        this.estilistas.forEach((e: Estilista) => {
          if (Array.isArray(e.servicios)) {
            e.servicios.forEach(s => setServicios.add(s));
          }
        });

        this.serviciosDisponibles = Array.from(setServicios);
      },

      error: (err) => {
        console.error(err);
        Swal.fire('Error', 'No se pudieron cargar los estilistas.', 'error');
      }
    });
  }
filtrarPorServicio(servicio: string) {
  this.servicioSeleccionado = servicio;

  if (servicio === 'Todos') {
    this.estilistasFiltrados = this.estilistas;
  } else {
    this.estilistasFiltrados = this.estilistas.filter(e =>
      e.servicios.includes(servicio)
    );
  }
}

  inactivar(estilista: Estilista) {
  this.estilistasService.citasPendientes(estilista.id).subscribe({
    next: (citas: number) => {

      // Si tiene citas pendientes → no permitir inactivar
      if (citas > 0) {
        Swal.fire(
          'Bloqueo',
          `Este estilista tiene ${citas} citas futuras.  
Debe reasignarlas o cancelarlas antes de inactivarlo.`,
          'error'
        );
        return;
      }

      // Confirmación de inactivación
      Swal.fire({
        title: "¿Inactivar estilista?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, inactivar",
        cancelButtonText: "Cancelar"
      }).then(r => {
        if (!r.isConfirmed) return;

        // Llamar al backend para inactivar
        this.estilistasService.inactivar(estilista.id).subscribe({
          next: () => {
            Swal.fire('Listo', 'El estilista ha sido inactivado.', 'success');
            this.cargarEstilistas();
          },
          error: () => {
            Swal.fire('Error', 'No se pudo inactivar el estilista.', 'error');
          }
        });
      });

    },

    error: () => {
      Swal.fire('Error', 'No se pudo verificar las citas pendientes.', 'error');
    }
  });
}

}
