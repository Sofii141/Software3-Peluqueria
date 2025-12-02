import { Component } from '@angular/core';
import { SidebarComponent } from './sidebar.component';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-private-layout',
  standalone: true,
  imports: [CommonModule, SidebarComponent, RouterOutlet],
  template: `
    <div class="private-layout">
      <app-sidebar></app-sidebar>
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styleUrls: ['./private-layout.component.css']
})
export class PrivateLayoutComponent {}
