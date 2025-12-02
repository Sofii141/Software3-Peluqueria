import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AgendaEstilistaService } from '../servicios/agenda-estilista.service';
import { ReservaDetalle, EstadoReserva } from '../modelos/reserva.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-reserva-detalle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reserva-detalle.component.html',
  styleUrls: ['./reserva-detalle.component.css']
})
export class ReservaDetalleComponent implements OnInit {

  reserva: ReservaDetalle | null = null;
  cargando: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private agendaService: AgendaEstilistaService
  ) {}

  ngOnInit(): void {
    const reservaId = Number(this.route.snapshot.paramMap.get('id'));

    if (!reservaId) {
      Swal.fire('Error', 'ID de reserva inválido', 'error');
      this.volver();
      return;
    }

    this.cargarReserva(reservaId);
  }

  cargarReserva(id: number): void {
    this.cargando = true;

    this.agendaService.getReservaDetalle(id).subscribe({
      next: (reserva) => {
        this.reserva = reserva;
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        this.volver();
      }
    });
  }

  // ========== ACCIONES ==========

  marcarCheckIn(): void {
    if (!this.reserva) return;

    Swal.fire({
      title: '¿Confirmar llegada del cliente?',
      text: `${this.reserva.clienteNombre}`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Sí, confirmar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed && this.reserva) {
        this.agendaService.marcarCheckIn(this.reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Check-in realizado',
              text: 'El cliente ha sido registrado.',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReserva(this.reserva!.id);
          }
        });
      }
    });
  }

  iniciarServicio(): void {
    if (!this.reserva) return;

    if (!this.reserva.puedeIniciar) {
      Swal.fire({
        icon: 'warning',
        title: 'No disponible',
        text: 'Solo puedes iniciar el servicio cuando llegue la hora de la cita.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    Swal.fire({
      title: '¿Iniciar servicio?',
      text: `${this.reserva.servicioNombre}`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Iniciar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed && this.reserva) {
        this.agendaService.iniciarServicio(this.reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Servicio iniciado',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReserva(this.reserva!.id);
          }
        });
      }
    });
  }

  finalizarServicio(): void {
    if (!this.reserva) return;

    Swal.fire({
      title: '¿Finalizar servicio?',
      text: 'Esta acción marcará el servicio como completado.',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Finalizar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed && this.reserva) {
        this.agendaService.finalizarServicio(this.reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Servicio finalizado',
              text: '¡Excelente trabajo!',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReserva(this.reserva!.id);
          }
        });
      }
    });
  }

  marcarNoShow(): void {
    if (!this.reserva) return;

    if (!this.reserva.puedeMarcarNoShow) {
      Swal.fire({
        icon: 'warning',
        title: 'No disponible',
        text: 'Solo puedes marcar No-Show después de 10 minutos de la hora acordada.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    Swal.fire({
      title: '¿Marcar como No-Show?',
      html: `
        <p>El cliente <strong>${this.reserva.clienteNombre}</strong> no se presentó a la cita.</p>
        <p class="text-muted">Esta acción no se puede deshacer.</p>
      `,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      confirmButtonText: 'Sí, marcar No-Show',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed && this.reserva) {
        this.agendaService.marcarNoShow(this.reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'info',
              title: 'Marcado como No-Show',
              text: 'La reserva ha sido actualizada.',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReserva(this.reserva!.id);
          }
        });
      }
    });
  }

  // ========== UTILIDADES ==========

  volver(): void {
    this.router.navigate(['/estilista/agenda']);
  }

  obtenerColorEstado(estado: EstadoReserva): string {
    const colores: { [key in EstadoReserva]: string } = {
      [EstadoReserva.PENDIENTE]: '#6c757d',
      [EstadoReserva.CONFIRMADA]: '#3B82F6',
      [EstadoReserva.EN_CURSO]: '#D4AF37',
      [EstadoReserva.COMPLETADA]: '#16A34A',
      [EstadoReserva.CANCELADA]: '#DC2626',
      [EstadoReserva.NO_SHOW]: '#b91c1c'
    };
    return colores[estado] || '#6c757d';
  }

  obtenerIconoEstado(estado: EstadoReserva): string {
    const iconos: { [key in EstadoReserva]: string } = {
      [EstadoReserva.PENDIENTE]: 'fa-clock',
      [EstadoReserva.CONFIRMADA]: 'fa-check-circle',
      [EstadoReserva.EN_CURSO]: 'fa-spinner',
      [EstadoReserva.COMPLETADA]: 'fa-check-double',
      [EstadoReserva.CANCELADA]: 'fa-times-circle',
      [EstadoReserva.NO_SHOW]: 'fa-user-times'
    };
    return iconos[estado] || 'fa-question-circle';
  }

  obtenerTextoEstado(estado: EstadoReserva): string {
    const textos: { [key in EstadoReserva]: string } = {
      [EstadoReserva.PENDIENTE]: 'Pendiente',
      [EstadoReserva.CONFIRMADA]: 'Confirmada',
      [EstadoReserva.EN_CURSO]: 'En Curso',
      [EstadoReserva.COMPLETADA]: 'Completada',
      [EstadoReserva.CANCELADA]: 'Cancelada',
      [EstadoReserva.NO_SHOW]: 'No Show'
    };
    return textos[estado] || 'Desconocido';
  }

  formatearFecha(fecha: string): string {
    const date = new Date(fecha + 'T00:00:00');
    return date.toLocaleDateString('es-ES', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  calcularTiempoRestante(): string {
    if (!this.reserva) return '';

    if (this.reserva.minutosParaInicio > 60) {
      const horas = Math.floor(this.reserva.minutosParaInicio / 60);
      return `Faltan ${horas} hora${horas > 1 ? 's' : ''}`;
    } else if (this.reserva.minutosParaInicio > 0) {
      return `Faltan ${this.reserva.minutosParaInicio} minutos`;
    } else if (this.reserva.minutosParaInicio >= -10) {
      return 'Es ahora';
    } else {
      return `Hace ${Math.abs(this.reserva.minutosParaInicio)} minutos`;
    }
  }

  // ========== GETTERS PARA TEMPLATE ==========

  get mostrarBotonCheckIn(): boolean {
    return this.reserva?.estado === EstadoReserva.PENDIENTE;
  }

  get mostrarBotonIniciar(): boolean {
    return this.reserva?.estado === EstadoReserva.CONFIRMADA;
  }

  get mostrarBotonFinalizar(): boolean {
    return this.reserva?.estado === EstadoReserva.EN_CURSO;
  }

  get mostrarBotonNoShow(): boolean {
    return this.reserva?.estado === EstadoReserva.CONFIRMADA;
  }

  get esEstadoFinal(): boolean {
    return this.reserva?.estado === EstadoReserva.COMPLETADA ||
      this.reserva?.estado === EstadoReserva.CANCELADA ||
      this.reserva?.estado === EstadoReserva.NO_SHOW;
  }

  // ========== ENUM PARA TEMPLATE ==========
  readonly EstadoReserva = EstadoReserva;
}
