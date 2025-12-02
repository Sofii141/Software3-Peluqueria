import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import Swal from 'sweetalert2';
import { EstilistasService } from '../servicios/estilistas.service';
import { EstilistaAgendaService } from '../servicios/estilista-agenda.service';
import { ValidacionesService } from '../servicios/validaciones.service';
import { Estilista } from '../modelos/estilista.model';
import { HorarioDia, HorarioDiaDisplay } from '../modelos/horario-dia';
import { BloqueoRango, BloqueoRangoResponse } from '../modelos/bloqueo-rango';

@Component({
  selector: 'app-configurar-horarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './configurar-horarios.component.html',
  styleUrls: ['./configurar-horarios.component.css']
})
export class ConfigurarHorariosComponent implements OnInit {

  estilistaId: number = 0;
  estilista: Estilista | null = null;

  // Tabs
  tabActiva: 'horario' | 'descansos' | 'bloqueos' = 'horario';

  // Horario Base
  diasSemana: HorarioDiaDisplay[] = [];

  // Descansos Fijos
  descansosFijos: HorarioDiaDisplay[] = [];
  nuevoDescanso: HorarioDiaDisplay = this.crearDescansoVacio();

  // Bloqueos
  bloqueos: BloqueoRangoResponse[] = [];
  nuevoBloqueo: BloqueoRango = this.crearBloqueoVacio();
  bloqueoEditando: BloqueoRangoResponse | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private estilistaService: EstilistasService,
    private agendaService: EstilistaAgendaService,
    private validacionesService: ValidacionesService
  ) {}

  ngOnInit(): void {
    this.estilistaId = Number(this.route.snapshot.paramMap.get('id'));

    if (!this.estilistaId) {
      Swal.fire('Error', 'ID de estilista inválido', 'error');
      this.router.navigate(['/admin/estilistas']);
      return;
    }

    this.cargarEstilista();
    this.cargarHorarioBase();
    this.cargarDescansosFijos();
    this.cargarBloqueos();
  }

  // ========== CARGA DE DATOS ==========

  cargarEstilista(): void {
    this.estilistaService.getEstilistaById(this.estilistaId).subscribe({
      next: (estilista: Estilista) => {
        this.estilista = estilista;
      },
      error: () => {
        this.router.navigate(['/admin/estilistas']);
      }
    });
  }

  cargarHorarioBase(): void {
    this.agendaService.getHorarioBase(this.estilistaId).subscribe({
      next: (horarios) => {
        this.diasSemana = this.mapearHorariosConNombres(horarios);
        this.completarDiasFaltantes();
      },
      error: () => {
        this.inicializarDiasSemana();
      }
    });
  }

  cargarDescansosFijos(): void {
    this.agendaService.getDescansosFijos(this.estilistaId).subscribe({
      next: (descansos) => {
        this.descansosFijos = this.mapearHorariosConNombres(descansos);
      },
      error: () => {
        this.descansosFijos = [];
      }
    });
  }

  cargarBloqueos(): void {
    this.agendaService.getBloqueosDiasLibres(this.estilistaId).subscribe({
      next: (bloqueos) => {
        this.bloqueos = bloqueos;
      },
      error: () => {
        this.bloqueos = [];
      }
    });
  }

  // ========== HORARIO BASE ==========

  inicializarDiasSemana(): void {
    const nombresDias = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

    this.diasSemana = nombresDias.map((nombre, index) => ({
      diaSemana: index,
      nombreDia: nombre,
      horaInicio: '09:00',
      horaFin: '18:00',
      esLaborable: index >= 1 && index <= 5
    }));
  }

  completarDiasFaltantes(): void {
    const nombresDias = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

    for (let i = 0; i < 7; i++) {
      if (!this.diasSemana.find(d => d.diaSemana === i)) {
        this.diasSemana.push({
          diaSemana: i,
          nombreDia: nombresDias[i],
          horaInicio: '09:00',
          horaFin: '18:00',
          esLaborable: false
        });
      }
    }

    this.diasSemana.sort((a, b) => a.diaSemana - b.diaSemana);
  }

  // ⭐ VALIDACIÓN MEJORADA: Verificar citas antes de cambiar día laborable
  async cambiarEstadoDia(dia: HorarioDiaDisplay): Promise<void> {
    // Si está marcando como NO laborable, verificar citas
    if (dia.esLaborable) {
      const tieneCitas = await this.validacionesService
        .tieneCitasEnDia(this.estilistaId, dia.diaSemana)
        .toPromise();

      if (tieneCitas) {
        Swal.fire({
          icon: 'error',
          title: 'No se puede desactivar',
          html: `
            <p><strong>${dia.nombreDia}</strong> tiene citas programadas.</p>
            <p>Debe cancelar o reasignar las citas antes de marcarlo como no laborable.</p>
          `,
          confirmButtonText: 'Entendido'
        });
        // Revertir el cambio
        dia.esLaborable = true;
        return;
      }
    }
  }

  guardarHorarioBase(): void {
    const diasInvalidos = this.diasSemana.filter(d =>
      d.esLaborable && (!d.horaInicio || !d.horaFin || d.horaInicio >= d.horaFin)
    );

    if (diasInvalidos.length > 0) {
      Swal.fire({
        icon: 'warning',
        title: 'Horarios inválidos',
        text: 'Los días laborables deben tener hora de inicio menor a hora de fin.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    const horariosParaEnviar = this.diasSemana.map(d => ({
      diaSemana: d.diaSemana,
      horaInicio: d.horaInicio,
      horaFin: d.horaFin,
      esLaborable: d.esLaborable
    }));

    this.agendaService.updateHorarioBase(this.estilistaId, horariosParaEnviar).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: 'Horario Base Actualizado',
          text: 'El horario semanal se guardó correctamente.',
          timer: 2000,
          showConfirmButton: false
        });
        this.cargarHorarioBase();
      },
      error: (err: HttpErrorResponse) => {
        console.error('Error al guardar horario base:', err);
      }
    });
  }

  // ========== DESCANSOS FIJOS ==========

  async agregarDescanso(): Promise<void> {
    const diaLaborable = this.diasSemana.find(d =>
      d.diaSemana === this.nuevoDescanso.diaSemana && d.esLaborable
    );

    if (!diaLaborable) {
      Swal.fire({
        icon: 'warning',
        title: 'Día no laborable',
        text: 'Solo puedes agregar descansos en días laborables.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    if (!this.nuevoDescanso.horaInicio || !this.nuevoDescanso.horaFin) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos incompletos',
        text: 'Debes especificar hora de inicio y fin del descanso.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    if (this.nuevoDescanso.horaInicio >= this.nuevoDescanso.horaFin) {
      Swal.fire({
        icon: 'warning',
        title: 'Horario inválido',
        text: 'La hora de inicio debe ser menor a la hora de fin.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    const yaExiste = this.descansosFijos.find(d => d.diaSemana === this.nuevoDescanso.diaSemana);
    if (yaExiste) {
      Swal.fire({
        icon: 'warning',
        title: 'Descanso duplicado',
        text: 'Ya existe un descanso configurado para este día.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    // ⭐ VALIDACIÓN: Verificar si hay citas en ese horario
    const tieneCitas = await this.validacionesService
      .tieneCitasEnDescanso(
        this.estilistaId,
        this.nuevoDescanso.diaSemana,
        this.nuevoDescanso.horaInicio,
        this.nuevoDescanso.horaFin
      )
      .toPromise();

    if (tieneCitas) {
      Swal.fire({
        icon: 'error',
        title: 'No se puede agregar',
        html: `
          <p>Hay citas programadas en el horario seleccionado.</p>
          <p><strong>${this.nuevoDescanso.horaInicio} - ${this.nuevoDescanso.horaFin}</strong></p>
          <p>Debe cancelar o reasignar las citas primero.</p>
        `,
        confirmButtonText: 'Entendido'
      });
      return;
    }

    const nombresDias = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    this.descansosFijos.push({
      ...this.nuevoDescanso,
      nombreDia: nombresDias[this.nuevoDescanso.diaSemana]
    });

    this.descansosFijos.sort((a, b) => a.diaSemana - b.diaSemana);
    this.nuevoDescanso = this.crearDescansoVacio();
  }

  eliminarDescanso(dia: number): void {
    Swal.fire({
      title: '¿Eliminar descanso?',
      text: 'Se eliminará el descanso configurado para este día.',
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.descansosFijos = this.descansosFijos.filter(d => d.diaSemana !== dia);
      }
    });
  }

  guardarDescansosFijos(): void {
    const descansosParaEnviar = this.descansosFijos.map(d => ({
      diaSemana: d.diaSemana,
      horaInicio: d.horaInicio,
      horaFin: d.horaFin,
      esLaborable: false
    }));

    this.agendaService.updateDescansosFijos(this.estilistaId, descansosParaEnviar).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: 'Descansos Actualizados',
          text: 'Los descansos fijos se guardaron correctamente.',
          timer: 2000,
          showConfirmButton: false
        });
        this.cargarDescansosFijos();
      },
      error: (err: HttpErrorResponse) => {
        console.error('Error al guardar descansos:', err);
      }
    });
  }

  // ========== BLOQUEOS (VACACIONES) ==========

  async agregarBloqueo(): Promise<void> {
    // Validar fechas
    if (!this.nuevoBloqueo.fechaInicio || !this.nuevoBloqueo.fechaFin) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos incompletos',
        text: 'Debes especificar fecha de inicio y fin.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    if (this.nuevoBloqueo.fechaInicio > this.nuevoBloqueo.fechaFin) {
      Swal.fire({
        icon: 'warning',
        title: 'Fechas inválidas',
        text: 'La fecha de inicio debe ser anterior o igual a la fecha de fin.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    // Validar que no sea una fecha pasada
    const hoy = new Date().toISOString().split('T')[0];
    if (this.nuevoBloqueo.fechaInicio < hoy) {
      Swal.fire({
        icon: 'warning',
        title: 'Fecha pasada',
        text: 'No puedes crear bloqueos en fechas pasadas.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    if (!this.nuevoBloqueo.razon || this.nuevoBloqueo.razon.trim() === '') {
      Swal.fire({
        icon: 'warning',
        title: 'Razón requerida',
        text: 'Debes especificar una razón para el bloqueo.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    // ⭐ VALIDACIÓN: Verificar si hay citas en ese rango
    const tieneCitas = await this.validacionesService
      .tieneCitasEnRango(
        this.estilistaId,
        this.nuevoBloqueo.fechaInicio,
        this.nuevoBloqueo.fechaFin
      )
      .toPromise();

    if (tieneCitas) {
      Swal.fire({
        icon: 'error',
        title: 'No se puede crear el bloqueo',
        html: `
          <p>Hay citas programadas en el rango de fechas seleccionado:</p>
          <p><strong>${this.formatearFecha(this.nuevoBloqueo.fechaInicio)}</strong> - <strong>${this.formatearFecha(this.nuevoBloqueo.fechaFin)}</strong></p>
          <p>Debe cancelar o reasignar las citas antes de crear el bloqueo.</p>
        `,
        confirmButtonText: 'Entendido'
      });
      return;
    }

    this.agendaService.createBloqueoDiasLibres(this.estilistaId, this.nuevoBloqueo).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: 'Bloqueo Creado',
          text: 'El bloqueo de días se creó correctamente.',
          timer: 2000,
          showConfirmButton: false
        });
        this.cargarBloqueos();
        this.nuevoBloqueo = this.crearBloqueoVacio();
      },
      error: (err: HttpErrorResponse) => {
        console.error('Error al crear bloqueo:', err);
      }
    });
  }

  editarBloqueo(bloqueo: BloqueoRangoResponse): void {
    this.bloqueoEditando = { ...bloqueo };
  }

  cancelarEdicionBloqueo(): void {
    this.bloqueoEditando = null;
  }

  async guardarEdicionBloqueo(): Promise<void> {
    if (!this.bloqueoEditando) return;

    // Validaciones
    if (this.bloqueoEditando.fechaInicio > this.bloqueoEditando.fechaFin) {
      Swal.fire({
        icon: 'warning',
        title: 'Fechas inválidas',
        text: 'La fecha de inicio debe ser anterior o igual a la fecha de fin.',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    // ⭐ VALIDACIÓN: Verificar si hay citas en el nuevo rango
    const tieneCitas = await this.validacionesService
      .tieneCitasEnRango(
        this.estilistaId,
        this.bloqueoEditando.fechaInicio,
        this.bloqueoEditando.fechaFin
      )
      .toPromise();

    if (tieneCitas) {
      Swal.fire({
        icon: 'error',
        title: 'No se puede actualizar',
        html: `
          <p>Hay citas programadas en el nuevo rango de fechas.</p>
          <p>Debe cancelar o reasignar las citas primero.</p>
        `,
        confirmButtonText: 'Entendido'
      });
      return;
    }

    const bloqueoParaEnviar: BloqueoRango = {
      fechaInicio: this.bloqueoEditando.fechaInicio,
      fechaFin: this.bloqueoEditando.fechaFin,
      razon: this.bloqueoEditando.razon
    };

    this.agendaService.updateBloqueoDiasLibres(
      this.estilistaId,
      this.bloqueoEditando.id,
      bloqueoParaEnviar
    ).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: 'Bloqueo Actualizado',
          text: 'El bloqueo se actualizó correctamente.',
          timer: 2000,
          showConfirmButton: false
        });
        this.cargarBloqueos();
        this.bloqueoEditando = null;
      },
      error: (err: HttpErrorResponse) => {
        console.error('Error al actualizar bloqueo:', err);
      }
    });
  }

  eliminarBloqueo(bloqueoId: number): void {
    Swal.fire({
      title: '¿Eliminar bloqueo?',
      html: `
        <p>Se eliminará este bloqueo de días libres.</p>
        <p class="text-muted">El estilista volverá a estar disponible en esas fechas.</p>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.agendaService.deleteBloqueoDiasLibres(this.estilistaId, bloqueoId).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Bloqueo Eliminado',
              text: 'El bloqueo se eliminó correctamente.',
              timer: 2000,
              showConfirmButton: false
            });
            this.cargarBloqueos();
          },
          error: (err: HttpErrorResponse) => {
            console.error('Error al eliminar bloqueo:', err);
          }
        });
      }
    });
  }

  // ========== UTILIDADES ==========

  cambiarTab(tab: 'horario' | 'descansos' | 'bloqueos'): void {
    this.tabActiva = tab;
  }

  mapearHorariosConNombres(horarios: HorarioDia[]): HorarioDiaDisplay[] {
    const nombresDias = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

    return horarios.map(h => ({
      ...h,
      nombreDia: nombresDias[h.diaSemana]
    }));
  }

  crearDescansoVacio(): HorarioDiaDisplay {
    return {
      diaSemana: 1, // Lunes por defecto
      nombreDia: 'Lunes',
      horaInicio: '13:00',
      horaFin: '14:00',
      esLaborable: false
    };
  }

  crearBloqueoVacio(): BloqueoRango {
    return {
      fechaInicio: '',
      fechaFin: '',
      razon: ''
    };
  }

  volver(): void {
    this.router.navigate(['/admin/estilistas']);
  }

  // Obtener solo días laborables para el selector de descansos
  get diasLaborables(): HorarioDiaDisplay[] {
    return this.diasSemana.filter(d => d.esLaborable);
  }

  // Formatear fecha para mostrar
  formatearFecha(fecha: string): string {
    const date = new Date(fecha + 'T00:00:00');
    return date.toLocaleDateString('es-ES', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  // ⭐ NUEVO: Obtener descripción del estado de un día
  getEstadoDia(dia: HorarioDiaDisplay): string {
    if (!dia.esLaborable) {
      return 'Día no laborable';
    }

    const descanso = this.descansosFijos.find(d => d.diaSemana === dia.diaSemana);
    if (descanso) {
      return `Laborable (Descanso: ${descanso.horaInicio} - ${descanso.horaFin})`;
    }

    return `Laborable (${dia.horaInicio} - ${dia.horaFin})`;
  }

  // ⭐ NUEVO: Verificar si un bloqueo está activo
  esBloqueActivo(bloqueo: BloqueoRangoResponse): boolean {
    const hoy = new Date().toISOString().split('T')[0];
    return bloqueo.fechaFin >= hoy;
  }

  // ⭐ NUEVO: Obtener clase CSS para bloqueo
  getClaseBloqueo(bloqueo: BloqueoRangoResponse): string {
    return this.esBloqueActivo(bloqueo) ? 'bloqueo-activo' : 'bloqueo-pasado';
  }

  // ⭐ NUEVO: Calcular duración de descanso
  calcularDuracion(inicio: string, fin: string): string {
    const [horaIni, minIni] = inicio.split(':').map(Number);
    const [horaFin, minFin] = fin.split(':').map(Number);

    const minutosIni = horaIni * 60 + minIni;
    const minutosFin = horaFin * 60 + minFin;
    const diferencia = minutosFin - minutosIni;

    const horas = Math.floor(diferencia / 60);
    const minutos = diferencia % 60;

    if (horas > 0 && minutos > 0) {
      return `${horas}h ${minutos}min`;
    } else if (horas > 0) {
      return `${horas}h`;
    } else {
      return `${minutos}min`;
    }
  }

  // ⭐ NUEVO: Calcular días bloqueados
  calcularDiasBloqueados(inicio: string, fin: string): string {
    const fechaIni = new Date(inicio + 'T00:00:00');
    const fechaFin = new Date(fin + 'T00:00:00');

    const diferencia = fechaFin.getTime() - fechaIni.getTime();
    const dias = Math.ceil(diferencia / (1000 * 60 * 60 * 24)) + 1;

    return dias === 1 ? '1 día' : `${dias} días`;
  }

  // ⭐ NUEVO: Obtener fecha mínima (hoy)
  getFechaMinima(): string {
    const hoy = new Date();
    const year = hoy.getFullYear();
    const month = String(hoy.getMonth() + 1).padStart(2, '0');
    const day = String(hoy.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
