import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['../login/login.component.css']
})
export class RegisterComponent {
  public userData = {
    username: '',
    nombreCompleto: '',
    email: '',
    password: '',
    telefono: ''
  };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    if (
      !this.userData.username ||
      !this.userData.nombreCompleto ||
      !this.userData.email ||
      !this.userData.password ||
      !this.userData.telefono
    ) {
      Swal.fire({
        icon: 'warning',
        title: 'Faltan datos',
        text: 'Por favor completa todos los campos.',
      });
      return;
    }

    this.authService.register(this.userData).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: '¡Registro exitoso!',
          text: 'Tu cuenta ha sido creada correctamente.',
          timer: 2000,
          showConfirmButton: false
        });

        this.router.navigate(['/login']);
      },
      error: (err) => {
        const errorMsg = err.error?.message || err.error?.errors?.[0]?.description || 'Error inesperado.';
        Swal.fire({
          icon: 'error',
          title: 'Error en el registro',
          text: errorMsg,
        });
      }
    });
  }
}
