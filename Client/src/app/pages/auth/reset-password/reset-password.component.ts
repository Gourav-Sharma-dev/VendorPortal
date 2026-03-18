import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import { ResetPasswordRequest } from '../../../models/auth.models';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    otp: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]]
  });

  loading = false;
  errorMsg = '';
  successMsg = '';
  showPwd = false;

  get f() { return this.form.controls; }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.auth.resetPassword(this.form.value as ResetPasswordRequest).subscribe({
      next: () => { this.successMsg = 'Password reset successful! Please sign in.'; this.loading = false; },
      error: (err: { error?: { message?: string } }) => {
        this.errorMsg = err.error?.message || 'Reset failed. Check your email and OTP.';
        this.loading = false;
      }
    });
  }
}

