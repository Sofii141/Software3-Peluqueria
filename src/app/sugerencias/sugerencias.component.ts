import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-sugerencias',
  standalone: true,
  imports: [CommonModule, FormsModule], 
  templateUrl: './sugerencias.component.html',
  styleUrls: ['./sugerencias.component.css']
})
export class SugerenciasComponent {
  public sugerencia: any = {
    nombre: '',
    descripcion: ''
  };

  enviarSugerencia(): void {
    console.log('Sugerencia enviada:', this.sugerencia);
    Swal.fire('¡Gracias!', 'Hemos recibido tu sugerencia.', 'success');
    
    this.sugerencia.nombre = '';
    this.sugerencia.descripcion = '';
  }
}