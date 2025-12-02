import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';


@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {

  role: string | null = null;
  isOpen = false; // sidebar móvil

    constructor(private authService: AuthService) {
      this.role = this.authService.getRole() ?? '';
    }

    toggleSidebar() {
      this.isOpen = !this.isOpen;
    }

    logout() {
      this.authService.logout();
    }
}