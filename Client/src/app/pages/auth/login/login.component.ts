import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import { LoginRequest } from '../../../models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  loading = false;
  errorMsg = '';
  showPwd = false;

  get f() { return this.form.controls; }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.errorMsg = '';
    this.auth.login(this.form.value as LoginRequest).subscribe({
      next: () => {
        const roles = this.auth.getRoles();
        if (roles.includes('Vendor')) this.router.navigate(['/app/vendor/dashboard']);
        else this.router.navigate(['/app/dashboard']);
      },
      error: (err: { error?: { message?: string } }) => {
        this.errorMsg = err.error?.message || 'Invalid credentials';
        this.loading = false;
      }
    });
  }
}

