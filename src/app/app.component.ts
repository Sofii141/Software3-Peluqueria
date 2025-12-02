import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';
import { SidebarComponent } from './layout/sidebar.component';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    HeaderComponent,
    FooterComponent,
    SidebarComponent,
    RouterOutlet,
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'primerProyecto';
  showSidebar = false;
  showHeader = true;
  showFooter = true;

  constructor(private router: Router, private authService: AuthService) {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        const url = event.urlAfterRedirects;
        const loggedIn = this.authService.isLoggedIn();

        const isLogin = url.includes('/login');
        const isRegister = url.includes('/registro');
        const isHome = url === '/';

        const isPublic = isLogin || isRegister || isHome;
        const isPrivate = loggedIn && !isPublic;

        this.showSidebar = isPrivate;
        this.showHeader = true;
        this.showFooter = isHome; // solo en "/"
      }
    });
  }
}
