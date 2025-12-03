import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  public credentials = {
    username: '',
    password: ''
  };

  public loginError: string | null = null;

  constructor(private authService: AuthService) {}

  onSubmit(): void {
    if (!this.credentials.username || !this.credentials.password) {
      this.loginError = 'Completa todos los campos.';
      return;
    }

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        Swal.fire({
          icon: 'success',
          title: `¡Bienvenido, ${response.userName || this.credentials.username}!`,
          text: 'Has iniciado sesión correctamente.',
          timer: 2000,
          showConfirmButton: false
        });

        // ✅ El token y navegación ya se manejan en AuthService
      },
      error: (err) => {
        console.error(err);
        this.loginError = err?.error?.message || 'Credenciales inválidas.';
        Swal.fire({
          icon: 'error',
          title: 'Error al iniciar sesión',
          text: this.loginError ?? 'Ocurrió un error inesperado.',
        });

      }
    });
  }
}
