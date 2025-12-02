import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['../login/login.component.css']
})
export class RegisterComponent {
  public userData = {
    username: '',
    email: '',
    password: ''
  };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(registerForm: NgForm): void {
    if (registerForm.invalid) {
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

        // ✅ Redirigir al login
        this.router.navigate(['/login']);
      },
      error: (err) => {
        const errorMsg = err.error?.errors?.[0]?.description || 'Error inesperado. Inténtalo de nuevo.';
        Swal.fire({
          icon: 'error',
          title: 'Error en el registro',
          text: errorMsg,
        });
      }
    });
  }
}
