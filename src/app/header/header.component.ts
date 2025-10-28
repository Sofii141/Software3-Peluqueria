import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {

  public currentUserRole$: Observable<string | null>;

  constructor(private authService: AuthService) {
    this.currentUserRole$ = this.authService.currentUserRole$;
  }

  logout(): void {
    this.authService.logout();
  }
}