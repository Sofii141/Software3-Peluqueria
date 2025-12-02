import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AgendaEstilistaService } from '../servicios/agenda-estilista.service';
import { ReservaDetalle, EstadoReserva } from '../modelos/reserva.model';
import Swal from 'sweetalert2';

import { jwtDecode } from 'jwt-decode';
import { AuthService } from '../../auth/auth.service';


@Component({
  selector: 'app-agenda-calendario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './agenda-calendario.component.html',
  styleUrls: ['./agenda-calendario.component.css']
})
export class AgendaCalendarioComponent implements OnInit {

  estilistaId: number = 0; // Obtener del token JWT
  fechaSeleccionada: string = '';
  reservas: ReservaDetalle[] = [];
  cargando: boolean = false;

  // Para el calendario
  mesActual: Date = new Date();
  diasDelMes: Date[] = [];
  nombreMes: string = '';

  constructor(
    private agendaService: AgendaEstilistaService,
    private router: Router,
    private authService: AuthService // <--- AGREGAR
  ) {}

  ngOnInit(): void {
    // Obtener estilistaId del token
    const token = this.authService.getToken();

    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        // Asumiendo que el token tiene un claim 'estilistaId' o 'nameid'
        this.estilistaId = parseInt(decodedToken.estilistaId || decodedToken.nameid);
      } catch (error) {
        console.error('Error decodificando token:', error);
        Swal.fire('Error', 'No se pudo obtener tu información de estilista', 'error');
        this.router.navigate(['/']);
        return;
      }
    }

    this.fechaSeleccionada = this.formatearFecha(new Date());
    this.generarCalendario();
    this.cargarReservas();
  }

  // ========== CALENDARIO ==========
  generarCalendario(): void {
    const year = this.mesActual.getFullYear();
    const month = this.mesActual.getMonth();

    this.nombreMes = this.mesActual.toLocaleDateString('es-ES', {
      month: 'long',
      year: 'numeric'
    });

    const primerDia = new Date(year, month, 1);
    const ultimoDia = new Date(year, month + 1, 0);

    this.diasDelMes = [];

    // Días del mes anterior (para completar la primera semana)
    const diaSemanaInicio = primerDia.getDay();
    for (let i = diaSemanaInicio - 1; i >= 0; i--) {
      const dia = new Date(year, month, -i);
      this.diasDelMes.push(dia);
    }

    // Días del mes actual
    for (let dia = 1; dia <= ultimoDia.getDate(); dia++) {
      this.diasDelMes.push(new Date(year, month, dia));
    }

    // Días del mes siguiente (para completar la última semana)
    const diasRestantes = 42 - this.diasDelMes.length; // 6 semanas x 7 días
    for (let i = 1; i <= diasRestantes; i++) {
      this.diasDelMes.push(new Date(year, month + 1, i));
    }
  }

  mesAnterior(): void {
    this.mesActual = new Date(this.mesActual.getFullYear(), this.mesActual.getMonth() - 1);
    this.generarCalendario();
  }

  mesSiguiente(): void {
    this.mesActual = new Date(this.mesActual.getFullYear(), this.mesActual.getMonth() + 1);
    this.generarCalendario();
  }

  seleccionarFecha(fecha: Date): void {
    this.fechaSeleccionada = this.formatearFecha(fecha);
    this.cargarReservas();
  }

  esFechaSeleccionada(fecha: Date): boolean {
    return this.formatearFecha(fecha) === this.fechaSeleccionada;
  }

  esMesActual(fecha: Date): boolean {
    return fecha.getMonth() === this.mesActual.getMonth();
  }

  esHoy(fecha: Date): boolean {
    const hoy = new Date();
    return this.formatearFecha(fecha) === this.formatearFecha(hoy);
  }

  tieneCitas(fecha: Date): boolean {
    // TODO: Implementar lógica para marcar días con citas
    return false;
  }

  // ========== CARGA DE RESERVAS ==========
  cargarReservas(): void {
    this.cargando = true;

    this.agendaService.getReservasPorFecha(this.estilistaId, this.fechaSeleccionada).subscribe({
      next: (reservas) => {
        this.reservas = reservas.sort((a, b) =>
          a.horaInicio.localeCompare(b.horaInicio)
        );
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        this.reservas = [];
      }
    });
  }

  // ========== ACCIONES DE RESERVA ==========
  verDetalle(reservaId: number): void {
    this.router.navigate(['/estilista/agenda/detalle', reservaId]);
  }

  marcarCheckIn(reserva: ReservaDetalle): void {
    Swal.fire({
      title: '¿Confirmar llegada del cliente?',
      text: `${reserva.clienteNombre} - ${reserva.servicioNombre}`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Sí, confirmar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.agendaService.marcarCheckIn(reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Check-in realizado',
              text: 'El cliente ha sido registrado.',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReservas();
          }
        });
      }
    });
  }

  iniciarServicio(reserva: ReservaDetalle): void {
    if (!reserva.puedeIniciar) {
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
      text: `${reserva.servicioNombre} - ${reserva.clienteNombre}`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Iniciar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.agendaService.iniciarServicio(reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Servicio iniciado',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReservas();
          }
        });
      }
    });
  }

  finalizarServicio(reserva: ReservaDetalle): void {
    Swal.fire({
      title: '¿Finalizar servicio?',
      text: 'Esta acción marcará el servicio como completado.',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Finalizar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.agendaService.finalizarServicio(reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Servicio finalizado',
              text: '¡Excelente trabajo!',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReservas();
          }
        });
      }
    });
  }

  marcarNoShow(reserva: ReservaDetalle): void {
    if (!reserva.puedeMarcarNoShow) {
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
                <p>El cliente <strong>${reserva.clienteNombre}</strong> no se presentó a la cita.</p>
        <p class="text-muted">Esta acción no se puede deshacer.</p>
      `,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      confirmButtonText: 'Sí, marcar No-Show',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.agendaService.marcarNoShow(reserva.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'info',
              title: 'Marcado como No-Show',
              text: 'La reserva ha sido actualizada.',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarReservas();
          }
        });
      }
    });
  }

  // ========== UTILIDADES ==========
  formatearFecha(fecha: Date): string {
    const year = fecha.getFullYear();
    const month = String(fecha.getMonth() + 1).padStart(2, '0');
    const day = String(fecha.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  obtenerNombreDia(fecha: Date): string {
    return fecha.toLocaleDateString('es-ES', { weekday: 'short' });
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

  calcularTiempoRestante(reserva: ReservaDetalle): string {
    if (reserva.minutosParaInicio > 60) {
      const horas = Math.floor(reserva.minutosParaInicio / 60);
      return `En ${horas}h`;
    } else if (reserva.minutosParaInicio > 0) {
      return `En ${reserva.minutosParaInicio} min`;
    } else if (reserva.minutosParaInicio >= -10) {
      return 'Ahora';
    } else {
      return `Hace ${Math.abs(reserva.minutosParaInicio)} min`;
    }
  }

  // ========== GETTERS PARA TEMPLATE ==========
  get reservasPendientes(): ReservaDetalle[] {
    return this.reservas.filter(r =>
      r.estado === EstadoReserva.PENDIENTE ||
      r.estado === EstadoReserva.CONFIRMADA
    );
  }

  get reservasEnCurso(): ReservaDetalle[] {
    return this.reservas.filter(r => r.estado === EstadoReserva.EN_CURSO);
  }

  get reservasCompletadas(): ReservaDetalle[] {
    return this.reservas.filter(r =>
      r.estado === EstadoReserva.COMPLETADA ||
      r.estado === EstadoReserva.NO_SHOW ||
      r.estado === EstadoReserva.CANCELADA
    );
  }

  get totalReservas(): number {
    return this.reservas.length;
  }

  get totalCompletadas(): number {
    return this.reservas.filter(r => r.estado === EstadoReserva.COMPLETADA).length;
  }

  get totalNoShow(): number {
    return this.reservas.filter(r => r.estado === EstadoReserva.NO_SHOW).length;
  }

  // ========== ENUM PARA TEMPLATE ==========
  readonly EstadoReserva = EstadoReserva;
}
