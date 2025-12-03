import { Component, OnInit } from '@angular/core';
import { NgFor, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EstilistasService } from '../estilistas.service';
import { Estilista } from '../estilista.model';
import Swal from 'sweetalert2';

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

  // → Ahora servicios son objetos con id + nombre
  serviciosDisponibles: { id: number, nombre: string }[] = [];

  servicioSeleccionado: number | 'Todos' = 'Todos';

  constructor(private estilistasService: EstilistasService) {}

  ngOnInit(): void {
    this.cargarEstilistas();
  }

  /** Cargar estilistas */
  cargarEstilistas(): void {
    this.estilistasService.listar().subscribe({
      next: (data: Estilista[]) => {
        this.estilistas = data;
        this.estilistasFiltrados = data;

        // Construir set de IDs de servicios
        const setIds = new Set<number>();

        data.forEach(e => {
          e.servicios?.forEach(id => {
            if (typeof id === 'number') {
              setIds.add(id);
            }
          });
        });

        // Convertir ids a objetos para mostrar su nombre
        this.serviciosDisponibles = Array.from(setIds).map(id => {
          return {
            id,
            nombre: this.getNombreServicio(id)   // ← traducir nombre
          }
        });
      },

      error: () => {
        Swal.fire('Error', 'No se pudieron cargar los estilistas.', 'error');
      }
    });
  }

  /** Traduce ID → nombre del servicio */
  getNombreServicio(id: number): string {
    const diccionario: any = {
      1: 'Corte',
      2: 'Color',
      3: 'Manicure',
      4: 'Pedicure',
      5: 'Peinados',
      6: 'Tratamientos'
    };

    return diccionario[id] || 'Desconocido';
  }

  /** Nombres para mostrar en tarjetas */
  obtenerNombresServicios(ids: number[]): string[] {
    return ids.map(id => this.getNombreServicio(id));
  }

  /** Filtrar por ID del servicio */
  filtrarPorServicio(servicioId: number | 'Todos'): void {

    this.servicioSeleccionado = servicioId;

    if (servicioId === 'Todos') {
      this.estilistasFiltrados = [...this.estilistas];
      return;
    }

    this.estilistasFiltrados = this.estilistas.filter(e =>
      Array.isArray(e.servicios) && e.servicios.includes(servicioId)
    );
  }

  /** Inactivar estilista */
  inactivar(estilista: Estilista): void {
    this.estilistasService.citasPendientes(estilista.id).subscribe({
      next: (citas: number) => {

        if (citas > 0) {
          Swal.fire(
            'Bloqueo',
            `Este estilista tiene ${citas} cita(s) pendiente(s).`,
            'error'
          );
          return;
        }

        Swal.fire({
          title: "¿Inactivar estilista?",
          icon: "warning",
          showCancelButton: true,
          confirmButtonText: "Sí, inactivar",
          cancelButtonText: "Cancelar"
        }).then(r => {
          if (!r.isConfirmed) return;

          this.estilistasService.inactivar(estilista.id).subscribe({
            next: () => {
              Swal.fire('Listo', 'Estilista inactivado.', 'success');
              this.cargarEstilistas();
            },
            error: () => {
              Swal.fire('Error', 'No se pudo inactivar el estilista.', 'error');
            }
          });
        });
      },

      error: () => {
        Swal.fire('Error', 'Error verificando citas pendientes.', 'error');
      }
    });
  }

}
