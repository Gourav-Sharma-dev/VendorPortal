import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  collapsed = false;

  get username(): string { return this.auth.username; }
  get currentRoles(): string { return this.auth.getRoles().join(', '); }
  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }
  logout(): void { this.auth.logout(); }
}

