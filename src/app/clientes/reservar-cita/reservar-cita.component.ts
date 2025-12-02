import { Component, OnInit } from '@angular/core';
import { ReservaService } from '../servicios/reserva.service';
import { AuthService } from '../../auth/auth.service';
import { Servicio, Estilista, Disponibilidad, ReservaRequest } from '../modelos/reserva.model';
import Swal from 'sweetalert2';
import {CommonModule} from "@angular/common";
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-reservar-cita',
  standalone: true,                 // asumo standalone por el error
  imports: [CommonModule, FormsModule],  // <--- agregar FormsModule aquí
  templateUrl: './reservar-cita.component.html',
  styleUrls: ['./reservar-cita.component.css']
})
export class ReservarCitaComponent implements OnInit {

  servicios: Servicio[] = [];
  estilistas: Estilista[] = [];
  disponibilidad: Disponibilidad | null = null;

  servicioSeleccionadoId?: number;
  estilistaSeleccionadoId?: number;
  fechaSeleccionada: string = '';
  horaSeleccionada: string = '';
  notas: string = '';

  clienteId?: number;

  constructor(private reservaService: ReservaService, private authService: AuthService) {}

  ngOnInit(): void {
    this.cargarServicios();
    const token = this.authService.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.clienteId = Number(payload['nameid'] || payload['sub']);
    }
  }

  cargarServicios(): void {
    this.reservaService.obtenerServicios().subscribe(s => this.servicios = s);
  }

  onServicioSeleccionado(): void {
    this.estilistas = [];
    this.estilistaSeleccionadoId = undefined;
    this.disponibilidad = null;
    if (this.servicioSeleccionadoId) {
      this.reservaService.obtenerEstilistasPorServicio(this.servicioSeleccionadoId).subscribe(e => this.estilistas = e);
    }
  }

  onEstilistaSeleccionado(): void {
    this.disponibilidad = null;
    this.fechaSeleccionada = '';
    this.horaSeleccionada = '';
  }

  onFechaSeleccionada(): void {
    if (this.estilistaSeleccionadoId && this.fechaSeleccionada) {
      this.reservaService.obtenerDisponibilidad(this.estilistaSeleccionadoId, this.fechaSeleccionada)
        .subscribe(d => this.disponibilidad = d);
    }
  }

  reservar(): void {
    if (!this.clienteId || !this.servicioSeleccionadoId || !this.estilistaSeleccionadoId || !this.fechaSeleccionada || !this.horaSeleccionada) {
      Swal.fire('Error', 'Complete todos los campos obligatorios para reservar', 'warning');
      return;
    }

    const reserva: ReservaRequest = {
      clienteId: this.clienteId,
      servicioId: this.servicioSeleccionadoId,
      estilistaId: this.estilistaSeleccionadoId,
      fecha: this.fechaSeleccionada,
      horaInicio: this.horaSeleccionada,
      notas: this.notas || undefined,
    };

    this.reservaService.crearReserva(reserva).subscribe({
      next: () => Swal.fire('Listo', '¡Cita creada con éxito!', 'success'),
      error: () => Swal.fire('Error', 'No se pudo crear la cita', 'error')
    });
  }
}
