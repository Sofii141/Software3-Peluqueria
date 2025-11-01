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

  // Este método se llama cuando el usuario envía el formulario.
  onSubmit(registerForm: NgForm): void {
    if (registerForm.invalid) {
      return;
    }

    this.authService.register(this.userData).subscribe({
      next: (response) => {
        Swal.fire({
          icon: 'success',
          title: `¡Registro Exitoso, ${response.userName}!`,
          text: 'Tu cuenta ha sido creada. ¡Bienvenido!',
          timer: 2000,
          showConfirmButton: false
        });
        this.router.navigate(['/']);
      },
      error: (err) => {
        const errorMsg = err.error?.errors?.[0]?.description || 'Este nombre de usuario o email ya están en uso.';
        Swal.fire({
          icon: 'error',
          title: 'Error en el Registro',
          text: errorMsg,
        });
      }
    });
  }
}