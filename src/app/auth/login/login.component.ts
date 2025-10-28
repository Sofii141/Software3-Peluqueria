import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  public credentials = {
    username: '',
    password: ''
  };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(loginForm: NgForm): void {
    if (loginForm.invalid) {
      return;
    }

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        Swal.fire({
          icon: 'success',
          title: `¡Bienvenido, ${response.userName}!`,
          text: 'Has iniciado sesión correctamente.',
          timer: 2000,
          showConfirmButton: false
        });
        this.router.navigate(['/']);
      },
      error: (err) => {
        Swal.fire({
          icon: 'error',
          title: 'Error de Autenticación',
          text: 'El nombre de usuario o la contraseña son incorrectos.',
        });
      }
    });
  }
}